// Modelo conceptual de la finca a partir del vídeo de dron (fotogramas/dron1).
// Coordenadas en metros: origen = centro de la casa principal, X = derecha del vídeo, Y = hacia el fondo (ladera arriba).
// Escala estimada con los coches (~4,5 m). Uso: node finca.cjs  -> crea los elementos en Revit y guarda ids en finca_ids.json
const net = require('net'), fs = require('fs');

const T = { // tipos de Revit (Proyecto4)
  muro: 1740,        // Genérico - 200 mm
  muroBloque: 9678,  // Exterior - Enlucido en ladrillo en bloque
  suelo: 1238,       // Genérico 150 mm
  sueloGrueso: 1239, // Genérico 300 mm
  cubierta: 1400,    // Genérico - 125 mm
  puerta: 67177,     // Simple-A ras 0915 x 2134
  ventana: 19024,    // Fijo 0915 x 1220
};
const mm = v => Math.round(v * 1000);
const P = (x, y, z = 0) => ({ x: mm(x), y: mm(y), z: mm(z) });
const loop = (pts, z = 0) => pts.map((p, i) => ({ p0: P(p[0], p[1], z), p1: P(pts[(i + 1) % pts.length][0], pts[(i + 1) % pts.length][1], z) }));
const rect = (cx, cy, w, d) => [[cx - w / 2, cy - d / 2], [cx + w / 2, cy - d / 2], [cx + w / 2, cy + d / 2], [cx - w / 2, cy + d / 2]];
const circle = (cx, cy, r, n = 16) => Array.from({ length: n }, (_, i) => [cx + r * Math.cos(2 * Math.PI * i / n), cy + r * Math.sin(2 * Math.PI * i / n)]);
// franja (camino) de ancho w a lo largo de una polilínea
function strip(line, w) {
  const L = [], R = [];
  line.forEach((p, i) => {
    const a = line[Math.max(i - 1, 0)], b = line[Math.min(i + 1, line.length - 1)];
    const dx = b[0] - a[0], dy = b[1] - a[1], n = Math.hypot(dx, dy);
    const ox = -dy / n * w / 2, oy = dx / n * w / 2;
    L.push([p[0] + ox, p[1] + oy]); R.push([p[0] - ox, p[1] - oy]);
  });
  return [...L, ...R.reverse()];
}

const floors = [], walls = [], roofs = [], openings = [];
const floor = (name, pts, off = 0, type = T.suelo, th = 150) =>
  floors.push({ name, category: 'OST_Floors', typeId: type, boundary: { outerLoop: loop(pts) }, thickness: th, baseLevel: 0, baseOffset: mm(off) });

// Edificio: muros + losa + cubierta plana con alero. doors/wins: [{wall:0..3, t:0..1}]
function building(name, cx, cy, w, d, h, opts = {}) {
  const pts = rect(cx, cy, w, d), first = walls.length;
  pts.forEach((p, i) => walls.push({ name, category: 'OST_Walls', typeId: opts.wallType || T.muro,
    locationLine: { p0: P(...p), p1: P(...pts[(i + 1) % 4]) }, thickness: 200, height: mm(h), baseLevel: 0, baseOffset: 0 }));
  floor(name + ' - losa', pts, 0.05);
  const ov = opts.overhang ?? 0.8;
  roofs.push({ name: name + ' - cubierta', category: 'OST_Roofs', typeId: T.cubierta,
    boundary: { outerLoop: loop(rect(cx, cy, w + 2 * ov, d + 2 * ov), h) }, thickness: 125, baseLevel: 0, baseOffset: mm(h) });
  const at = (wi, t) => { const a = pts[wi], b = pts[(wi + 1) % 4]; return [a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t]; };
  (opts.doors || []).forEach(o => openings.push({ kind: 'door', wallIdx: first + o.wall, xy: at(o.wall, o.t) }));
  (opts.wins || []).forEach(o => openings.push({ kind: 'win', wallIdx: first + o.wall, xy: at(o.wall, o.t) }));
}

// ---------- TERRENO ----------
floor('Terreno - finca', [[-70, -75], [70, -75], [75, 60], [-70, 62]], -0.25, T.sueloGrueso, 300);

// ---------- CASA PRINCIPAL (18 x 9 m) + corredor frontal ----------
building('Casa principal', 0, 0, 18, 9, 3.0, {
  wallType: T.muroBloque, overhang: 1.2,
  doors: [{ wall: 0, t: 0.5 }],
  wins: [{ wall: 0, t: 0.2 }, { wall: 0, t: 0.35 }, { wall: 0, t: 0.68 }, { wall: 0, t: 0.82 }, { wall: 1, t: 0.5 }, { wall: 3, t: 0.5 }, { wall: 2, t: 0.3 }, { wall: 2, t: 0.7 }],
});
floor('Casa principal - corredor', rect(0, -6, 18, 3), 0.05);

// ---------- CAMINOS ----------
floor('Anden central', strip([[0, -7.5], [-5, -16], [-12, -30]], 2.5), 0.03);
floor('Camino pavimentado inferior', strip([[-45, -36], [-25, -32], [-12, -30], [5, -27], [15, -24]], 3.5), 0.03);
floor('Camino de tierra', strip([[15, -24], [22, -14], [28, -2], [34, 10], [42, 22], [52, 28]], 4), 0.02);
floor('Sendero jardin', strip([[-12, -30], [-16, -40], [-15, -45]], 1.5), 0.03);
floor('Sendero superior', strip([[8, 6], [10, 18], [12, 30]], 1.5), 0.03);
floor('Explanada superior', rect(38, 32, 30, 14), 0.02);
floor('Parqueadero', rect(16, -8, 10, 7), 0.02);

// ---------- OTRAS CONSTRUCCIONES ----------
building('Casa blanca', 48, 18, 10, 6, 2.8, { doors: [{ wall: 0, t: 0.3 }], wins: [{ wall: 0, t: 0.7 }, { wall: 3, t: 0.5 }] });
building('Casa izquierda (techo gris)', -30, 6, 10, 8, 2.8, { doors: [{ wall: 0, t: 0.5 }], wins: [{ wall: 0, t: 0.2 }, { wall: 0, t: 0.8 }] });
building('Casa izquierda 2', -50, 12, 7, 6, 2.6, { doors: [{ wall: 0, t: 0.5 }] });
building('Construccion fondo', 14, 46, 7, 4, 2.6, { doors: [{ wall: 0, t: 0.5 }] });
building('Kiosco fondo derecha', 55, 50, 7, 5, 2.6, { doors: [{ wall: 0, t: 0.5 }] });
building('Portada techo rojo', -38, -31, 4, 3, 2.6, { overhang: 0.5 });
building('Invernadero', -24, -60, 8, 5, 2.4, { overhang: 0.3 });

// ---------- SANTUARIO / MONUMENTO con cerca ----------
floor('Santuario - plataforma', rect(26, 30, 9, 7), 0.2);
building('Santuario', 26, 30, 2.5, 2.5, 2.2, { overhang: 0.4 });
rect(26, 30, 9, 7).forEach((p, i, a) => walls.push({ name: 'Santuario - cerca', category: 'OST_Walls', typeId: T.muro,
  locationLine: { p0: P(...p), p1: P(...a[(i + 1) % 4]) }, thickness: 200, height: 900, baseLevel: 0, baseOffset: 0 }));

// ---------- ELEMENTOS CIRCULARES / AGUA ----------
floor('Jardinera circular', circle(10, 30, 3.5), 0.4);
floor('Jardinera pequena', circle(6, 12, 2), 0.4);
floor('Tanque circular', circle(-15, -48, 2.8), 0.6);
floor('Estanque', rect(2, -50, 10, 6), -0.03);

module.exports = { floors, walls, roofs, openings, T };

// ---------- ENVÍO A REVIT ----------
function send(method, params) {
  return new Promise((res, rej) => {
    const s = net.connect(8080, '127.0.0.1', () => s.write(JSON.stringify({ jsonrpc: '2.0', method, params, id: String(Date.now()) })));
    let b = ''; s.on('data', d => { b += d; try { const r = JSON.parse(b); s.end(); r.error ? rej(new Error(r.error.message)) : res(r.result); } catch { } });
    s.on('error', rej); setTimeout(() => rej(new Error('timeout ' + method)), 120000);
  });
}
async function batch(method, items, size = 10) {
  const ids = [];
  for (let i = 0; i < items.length; i += size) {
    const chunk = items.slice(i, i + size);
    try {
      const r = await send(method, { data: chunk.map(({ name, ...x }) => (method === 'create_line_based_element' ? x : { name, ...x })) });
      if (!r.Success) throw new Error(method + ': ' + r.Message);
      ids.push(...r.Response);
    } catch (e) { // el plugin corta a los pocos segundos, pero Revit termina de crear el elemento
      console.log('  aviso:', chunk.map(c => c.name).join(', '), '->', e.message.slice(0, 60));
      ids.push(...chunk.map(() => null)); await new Promise(r => setTimeout(r, 4000));
    }
  }
  return ids;
}
if (require.main === module) (async () => {
  const out = {};
  out.floors = await batch('create_surface_based_element', floors.slice(+(process.env.SKIP_FLOORS||0)), 1); console.log('suelos/caminos', out.floors.length);
  out.walls = await batch('create_line_based_element', walls, 4); console.log('muros', out.walls.length);
  out.roofs = await batch('create_surface_based_element', roofs, 2); console.log('cubiertas', out.roofs.length);
  const pe = openings.map(o => o.kind === 'door'
    ? { name: 'Puerta', typeId: T.puerta, locationPoint: P(o.xy[0], o.xy[1], 0), width: 915, height: 2134, baseLevel: 0, baseOffset: 0, hostWallId: out.walls[o.wallIdx] ?? undefined }
    : { name: 'Ventana', typeId: T.ventana, locationPoint: P(o.xy[0], o.xy[1], 1.0), width: 915, height: 1220, baseLevel: 0, baseOffset: 1000, hostWallId: out.walls[o.wallIdx] ?? undefined });
  out.openings = await batch('create_point_based_element', pe, 4); console.log('puertas/ventanas', out.openings.length);
  fs.writeFileSync(__dirname + '/finca_ids.json', JSON.stringify(out, null, 1));
  console.log('OK -> finca_ids.json');
})().catch(e => { console.error('ERROR', e.message); process.exit(1); });
