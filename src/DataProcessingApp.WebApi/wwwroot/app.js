const THEME_KEY = 'dpa-theme';
const media = matchMedia('(prefers-color-scheme: dark)');

let scenarios = [];
let current = null;
let computing = false;
let sweepCache = null;

async function fetchJson(url, options) {
  const res = await fetch(url, options);
  if (!res.ok && res.status !== 400) {
    throw new Error(url + ' failed: ' + res.status);
  }
  return res.json();
}

function post(url, body) {
  return fetchJson(url, {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify(body)
  });
}

function el(tag, className, text) {
  const node = document.createElement(tag);
  if (className) node.className = className;
  if (text !== undefined) node.textContent = text;
  return node;
}

function cssVar(name) {
  return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
}

function formatNumber(value) {
  if (!Number.isFinite(value)) return String(value);
  const rounded = Math.round(value * 100000) / 100000;
  return rounded.toLocaleString('en-US', { maximumFractionDigits: 5 });
}

function seriesBased() {
  return current.seriesBased;
}

function isAgeInput(input) {
  return input.name.startsWith('age');
}

function resolvedTheme(mode) {
  if (mode === 'system') return media.matches ? 'dark' : 'light';
  return mode;
}

function applyTheme(mode) {
  document.documentElement.dataset.theme = resolvedTheme(mode);
  for (const button of document.querySelectorAll('.theme-switch button')) {
    button.setAttribute('aria-pressed', String(button.dataset.mode === mode));
  }
}

function initTheme() {
  const mode = localStorage.getItem(THEME_KEY) || 'system';
  applyTheme(mode);

  for (const button of document.querySelectorAll('.theme-switch button')) {
    button.addEventListener('click', () => {
      localStorage.setItem(THEME_KEY, button.dataset.mode);
      applyTheme(button.dataset.mode);
      renderChartFromCache();
    });
  }

  media.addEventListener('change', () => {
    if ((localStorage.getItem(THEME_KEY) || 'system') === 'system') {
      applyTheme('system');
      renderChartFromCache();
    }
  });
}

async function init() {
  initTheme();

  scenarios = await fetchJson('/api/scenarios');
  const seriesData = await fetchJson('/api/series');
  const seriesSelect = document.getElementById('series');
  for (const s of seriesData.series) {
    seriesSelect.append(el('option', null, s));
  }
  seriesSelect.value = '2010CM';

  const sidebar = document.getElementById('scenarios');
  let lastCategory = null;
  for (const scenario of scenarios) {
    if (scenario.category !== lastCategory) {
      sidebar.append(el('div', 'group-label', scenario.category));
      lastCategory = scenario.category;
    }
    const button = el('button', 'scenario-btn');
    button.dataset.name = scenario.name;
    button.append(el('span', 'name', scenario.title));
    button.append(el('span', 'cite', scenario.citation.replace(/^IRS /, '')));
    button.addEventListener('click', () => selectScenario(scenario.name));
    sidebar.append(button);
  }

  document.getElementById('run').addEventListener('click', compute);
  seriesSelect.addEventListener('change', compute);
  document.getElementById('compare').addEventListener('change', compute);
  document.getElementById('chart-label').addEventListener('change', () => renderChartFromCache());

  selectScenario(scenarios[0].name);
}

function selectScenario(name) {
  current = scenarios.find(s => s.name === name);
  for (const button of document.querySelectorAll('.scenario-btn')) {
    button.classList.toggle('active', button.dataset.name === name);
  }

  document.getElementById('scenario-title').textContent = current.title;
  document.getElementById('scenario-purpose').textContent = current.purpose;
  document.getElementById('scenario-citation').textContent = 'Source: ' + current.citation;

  document.getElementById('series-wrap').hidden = !seriesBased();
  document.getElementById('compare-wrap').hidden = !seriesBased();

  const inputs = document.getElementById('inputs');
  inputs.replaceChildren();
  for (const input of current.inputs) {
    inputs.append(inputControl(input));
  }

  compute();
}

function inputControl(input) {
  const wrap = el('label');
  wrap.title = input.help;
  wrap.append(el('span', null, input.label));
  let control;
  if (input.kind === 'choice') {
    control = el('select');
    control.dataset.name = input.name;
    for (const choice of input.choices) {
      const option = el('option', null, choice);
      option.value = choice;
      control.append(option);
    }
    control.value = input.default;
    control.addEventListener('change', compute);
  } else {
    control = el('input');
    control.type = 'number';
    control.step = input.kind === 'rate' ? '0.2' : '1';
    if (input.kind === 'rate') {
      control.min = String(input.minRate);
      control.max = String(input.maxRate);
    } else {
      control.min = String(input.minInt);
      control.max = String(input.maxInt);
    }
    control.value = input.default;
    control.dataset.name = input.name;
    control.addEventListener('change', compute);
  }
  control.id = 'input-' + input.name;
  control.title = input.help;
  wrap.append(control);
  return wrap;
}

function gatherInputs() {
  const values = {};
  for (const input of current.inputs) {
    const control = document.getElementById('input-' + input.name);
    if (control && control.value !== '') {
      values[input.name] = control.value;
    }
  }
  return values;
}

async function compute() {
  if (!current || computing) return;
  computing = true;
  const run = document.getElementById('run');
  const errorBox = document.getElementById('error');
  run.disabled = true;
  run.textContent = 'Computing…';
  errorBox.textContent = '';

  const series = document.getElementById('series').value;
  const compare = document.getElementById('compare').checked && seriesBased();
  const inputs = gatherInputs();

  try {
    const result = await post('/api/calc', { scenario: current.name, series, inputs });
    if (result.error) {
      errorBox.textContent = result.error;
      document.getElementById('result').hidden = true;
      document.getElementById('chart-card').hidden = true;
      sweepCache = null;
      return;
    }
    renderResult(result);

    const hasAge = current.inputs.some(isAgeInput);
    if (hasAge) {
      const sweep = await post('/api/sweep/age', { scenario: current.name, series, inputs, compare });
      renderChartFromCache(sweep);
      document.getElementById('chart-card').hidden = !sweep.curves.length;
      document.getElementById('chart-note').textContent = chartNote(compare);
    } else {
      document.getElementById('chart-card').hidden = true;
      sweepCache = null;
    }
  } finally {
    computing = false;
    run.disabled = false;
    run.textContent = 'Compute';
  }
}

function chartNote(compare) {
  const diagonal = current.inputs.filter(isAgeInput).length > 1;
  const sweep = diagonal
    ? 'Age sweep along the diagonal (older life = younger life). '
    : 'Age sweep of the measuring life. ';
  const gaps = compare
    ? 'Curves cover every age with a published factor; gaps are cells outside the published grid.'
    : 'Points cover every age with a published factor.';
  return sweep + gaps;
}

function renderResult(result) {
  const box = document.getElementById('result');
  box.replaceChildren();
  box.hidden = false;

  const primaryLabel = (result.values.find(v => v.label.startsWith('Remainder')) || {}).label;

  const values = el('div', 'values');
  for (const value of result.values) {
    const card = el('div', value.label === primaryLabel ? 'value primary' : 'value');
    card.append(el('div', 'label', value.label));
    card.append(el('div', 'number', formatNumber(value.value)));
    if (value.note) {
      card.append(el('div', value.note.includes('always 1') ? 'note weak' : 'note', value.note));
    }
    values.append(card);
  }
  box.append(values);

  const tableWrap = el('div', 'table-wrap');
  const table = el('table');
  const headRow = el('tr');
  headRow.append(el('th', null, 'Value'), el('th', 'num', 'Factor'), el('th', null, 'Based on'));
  const head = el('thead');
  head.append(headRow);
  table.append(head);
  const body = el('tbody');
  for (const value of result.values) {
    const row = el('tr');
    row.append(el('td', null, value.label));
    row.append(el('td', 'num', formatNumber(value.value)));
    row.append(el('td', null, value.note || ''));
    body.append(row);
  }
  table.append(body);
  tableWrap.append(table);
  box.append(tableWrap);
}

function renderChartFromCache(sweep) {
  if (sweep) sweepCache = sweep;
  if (!sweepCache) return;
  renderChart(sweepCache);
}

function nearestPoint(points, age) {
  let best = null;
  let bestDistance = Infinity;
  for (const p of points) {
    const distance = Math.abs(p.x - age);
    if (distance < bestDistance) {
      best = p;
      bestDistance = distance;
    }
  }
  return best;
}

function renderChart(sweep) {
  const labels = sweep.labels || [];
  const labelSelect = document.getElementById('chart-label');
  const previous = labelSelect.value;
  labelSelect.replaceChildren();
  for (const label of labels) {
    const option = el('option', null, label);
    option.value = label;
    labelSelect.append(option);
  }
  const preferred = labels.find(l => l.startsWith('Remainder')) || labels[0] || '';
  labelSelect.value = previous && labels.includes(previous) ? previous : preferred;

  const selectedIndex = Math.max(0, labels.indexOf(labelSelect.value));
  const curves = sweep.curves.map(curve => ({
    series: curve.series,
    color: cssVar(curve.series ? '--series-' + curve.series.replace('CM', 'cm').toLowerCase() : '--series-single'),
    points: curve.points.map(p => ({ x: p.x, y: p.y[selectedIndex] })).filter(p => p.y !== undefined)
  })).filter(curve => curve.points.length > 1);

  const box = document.getElementById('chart');
  box.replaceChildren();
  if (!curves.length) return;

  const width = 820, height = 320;
  const margin = { left: 56, right: 14, top: 12, bottom: 30 };
  const innerW = width - margin.left - margin.right;
  const innerH = height - margin.top - margin.bottom;

  const allY = curves.flatMap(c => c.points.map(p => p.y));
  const maxX = Math.max(...curves.flatMap(c => c.points.map(p => p.x)));
  const maxY = Math.max(...allY, 1e-9);
  const niceMax = maxY / 1.05;

  const sx = x => margin.left + (x / maxX) * innerW;
  const sy = y => margin.top + innerH - (y / niceMax) * innerH;

  const grid = cssVar('--chart-grid');
  const tickText = cssVar('--dim');

  const svg = [];
  svg.push(`<svg viewBox="0 0 ${width} ${height}" role="img" aria-label="Factor vs age chart">`);
  svg.push(`<defs>`);
  curves.forEach((curve, i) => {
    svg.push(
      `<linearGradient id="grad-${i}" x1="0" y1="0" x2="0" y2="1">`,
      `<stop offset="0%" stop-color="${curve.color}" stop-opacity="0.22"/>`,
      `<stop offset="100%" stop-color="${curve.color}" stop-opacity="0"/>`,
      `</linearGradient>`);
  });
  svg.push(`</defs>`);

  const yTicks = 4;
  for (let i = 0; i <= yTicks; i++) {
    const value = (niceMax / yTicks) * i;
    const y = sy(value);
    svg.push(`<line x1="${margin.left}" y1="${y}" x2="${width - margin.right}" y2="${y}" stroke="${grid}" stroke-width="1"/>`);
    svg.push(`<text x="${margin.left - 8}" y="${y + 4}" fill="${tickText}" font-size="11" text-anchor="end">${formatNumber(value)}</text>`);
  }

  const xStep = maxX > 150 ? 30 : 20;
  for (let x = 0; x <= maxX; x += xStep) {
    svg.push(`<text x="${sx(x)}" y="${height - 8}" fill="${tickText}" font-size="11" text-anchor="middle">${x}</text>`);
  }
  svg.push(`<text x="${width - margin.right}" y="${height - 8}" fill="${tickText}" font-size="11" text-anchor="end">age</text>`);

  curves.forEach((curve, i) => {
    const polyline = curve.points.map(p => `${sx(p.x).toFixed(1)},${sy(p.y).toFixed(1)}`).join(' ');
    const area = `${sx(curve.points[0].x).toFixed(1)},${(margin.top + innerH).toFixed(1)} `
      + polyline
      + ` ${sx(curve.points[curve.points.length - 1].x).toFixed(1)},${(margin.top + innerH).toFixed(1)}`;
    svg.push(`<polygon points="${area}" fill="url(#grad-${i})" stroke="none"/>`);
    svg.push(`<polyline points="${polyline}" fill="none" stroke="${curve.color}" stroke-width="2"/>`);
    for (const p of curve.points) {
      svg.push(`<circle cx="${sx(p.x).toFixed(1)}" cy="${sy(p.y).toFixed(1)}" r="2.4" fill="${curve.color}"><title>age ${p.x}: ${formatNumber(p.y)}</title></circle>`);
    }
  });

  svg.push(`<line id="crosshair" x1="0" y1="${margin.top}" x2="0" y2="${margin.top + innerH}" stroke="${tickText}" stroke-width="1" stroke-dasharray="3 3" opacity="0"/>`);
  svg.push('</svg>');
  box.innerHTML = svg.join('');

  const oldLegend = document.querySelector('.legend');
  if (oldLegend) oldLegend.remove();
  if (curves.length > 1) {
    const legend = el('div', 'legend');
    for (const curve of curves) {
      const item = el('span');
      const swatch = el('span', 'swatch');
      swatch.style.background = curve.color;
      item.append(swatch, document.createTextNode(curve.series));
      legend.append(item);
    }
    box.parentNode.insertBefore(legend, box);
  }

  attachCrosshair(box, curves, sx, sy, maxX, margin, innerW, width);
}

function attachCrosshair(box, curves, sx, sy, maxX, margin, innerW, width) {
  const svg = box.querySelector('svg');
  const line = svg.querySelector('#crosshair');
  const tip = el('div', 'chart-tip');
  tip.hidden = true;
  box.append(tip);

  function hide() {
    tip.hidden = true;
    line.setAttribute('opacity', '0');
  }

  svg.addEventListener('mousemove', event => {
    const rect = svg.getBoundingClientRect();
    const xSvg = (event.clientX - rect.left) * (width / rect.width);
    const age = Math.round((xSvg - margin.left) / innerW * maxX);
    if (age < 0 || age > maxX) {
      hide();
      return;
    }

    const samples = curves.map(curve => ({ curve, point: nearestPoint(curve.points, age) })).filter(s => s.point);
    if (!samples.length) {
      hide();
      return;
    }

    line.setAttribute('x1', sx(samples[0].point.x));
    line.setAttribute('x2', sx(samples[0].point.x));
    line.setAttribute('opacity', '0.55');

    tip.replaceChildren();
    tip.append(el('div', 'tip-head', 'age ' + samples[0].point.x));
    for (const sample of samples) {
      const row = el('div', 'tip-row');
      const dot = el('span', 'dot');
      dot.style.background = sample.curve.color;
      row.append(dot);
      row.append(el('span', null, (sample.curve.series ? sample.curve.series + ' ' : '') + formatNumber(sample.point.y)));
      tip.append(row);
    }

    tip.hidden = false;
    const boxRect = box.getBoundingClientRect();
    const left = Math.min(
      Math.max((event.clientX - boxRect.left) + 14, 4),
      boxRect.width - tip.offsetWidth - 6);
    const top = Math.min(
      Math.max((event.clientY - boxRect.top) - tip.offsetHeight - 10, 4),
      boxRect.height - tip.offsetHeight - 4);
    tip.style.left = left + 'px';
    tip.style.top = top + 'px';
  });

  svg.addEventListener('mouseleave', hide);
}

init().catch(err => {
  document.getElementById('error').textContent = 'Failed to load: ' + err.message;
});
