// Etapa 4b: coloca la vegetación. Ejemplares del vídeo + bosque alrededor sobre el toposólido.
const { runCode, dataFile } = require('./realismo.cjs');
const finca = require('./finca.cjs');

// misma función de altura que etapa_terreno.cjs
const T0 = require('./terreno_h.cjs'); const { X0, X1, Y0, Y1 } = T0;
const h = (x, y) => (T0.h(x, y) === T0.BASE ? -0.25 : T0.h(x, y)) - 0.3; // enterrar un poco la base
let seed = 7; const rnd = () => (seed = (seed * 16807) % 2147483647) / 2147483647;

// obstáculos: edificios (rect de cubierta + 1 m) y suelos de caminos/estanque (polígonos)
const rects = finca.roofs.map(r => { const xs = r.boundary.outerLoop.map(s => s.p0.x / 1000), ys = r.boundary.outerLoop.map(s => s.p0.y / 1000);
  return [Math.min(...xs) - 1, Math.min(...ys) - 1, Math.max(...xs) + 1, Math.max(...ys) + 1]; });
const polys = finca.floors.filter(f => !/Terreno/.test(f.name)).map(f => f.boundary.outerLoop.map(s => [s.p0.x / 1000, s.p0.y / 1000]));
const inPoly = (x, y, P) => { let c = false; for (let i = 0, j = P.length - 1; i < P.length; j = i++) { const [xi, yi] = P[i], [xj, yj] = P[j]; if ((yi > y) !== (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi) c = !c; } return c; };
const free = (x, y, m = 1.5) => !rects.some(([a, b, c, d]) => x > a - m && x < c + m && y > b - m && y < d + m)
  && ![[0, 0], [m, 0], [-m, 0], [0, m], [0, -m]].some(([ox, oy]) => polys.some(P => inPoly(x + ox, y + oy, P)));

const T = [];
const add = (fam, x, y, force) => { if (force || free(x, y)) T.push([fam, x, y, h(x, y)]); };
// --- ejemplares vistos en el vídeo ---
add('Finca Pino', 7, -8, true); add('Finca Pino', -9, 16); add('Finca Pino', 18, 40);
[[-10, -7], [-19, -17], [-27, -12], [-4, 20]].forEach(([x, y]) => add('Finca Arbol claro', x, y));
[-36, -28, -20, -9, 1, 9].forEach(x => add('Finca Cipres', x, -27 + (x + 36) * 0.12 + 3.5));
[-40, -30, -16].forEach(x => add('Finca Cipres', x, -38));
[[-5, -37], [3, -34], [10, -33], [-2, -42], [16, -32]].forEach(([x, y]) => add('Finca Palma', x, y));
[[5, -4.2], [8, -4.5], [11, -3], [10.5, 1.5], [-7.5, -4.6]].forEach(([x, y]) => add('Finca Buganvilla', x, y, true));
[[-45, -52], [32, -30], [44, -14], [58, -32], [-55, -20], [62, 5]].forEach(([x, y]) => add('Finca Arbol frondoso', x, y));
// arbustos dispersos en el césped
for (let i = 0; i < 70; i++) add('Finca Arbusto', X0 + 8 + rnd() * (X1 - X0 - 16), Y0 + 8 + rnd() * (Y1 - Y0 - 16));
// zona de platanera / palmas abajo a la derecha
for (let i = 0; i < 25; i++) add('Finca Palma', 30 + rnd() * 42, -72 + rnd() * 25);
// borde de bosque dentro de la finca (izquierda y fondo)
for (let y = Y0 + 5; y < Y1; y += 7) for (let x = X0 + 2; x < X0 + 14; x += 7) add(rnd() < 0.5 ? 'Finca Arbol bosque' : 'Finca Arbol frondoso', x + rnd() * 3, y + rnd() * 3);
for (let x = X0 + 14; x < X1; x += 8) add(rnd() < 0.6 ? 'Finca Arbol bosque' : 'Finca Arbol frondoso', x + rnd() * 3, Y1 - 4 - rnd() * 6);
// bosque exterior
for (let x = -160; x <= 165; x += 11) for (let y = -140; y <= 190; y += 11) {
  const px = x + (rnd() - 0.5) * 8, py = y + (rnd() - 0.5) * 8;
  if (px > X0 - 3 && px < X1 + 3 && py > Y0 - 3 && py < Y1 + 3) continue;
  if (rnd() < 0.88) add(rnd() < 0.55 ? 'Finca Arbol bosque' : 'Finca Arbol frondoso', px, py, true);
}

const f = dataFile('arboles.txt', T.map(t => [t[0], ...t.slice(1).map(v => Math.round(v * 1000))].join('|')));
const CHUNK = 250;
(async () => {
  let total = 0;
  for (let i = 0; i < T.length; i += CHUNK) {
    const r = await runCode(`var lvl = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().First(l => Math.Abs(l.Elevation) < 0.001);
var syms = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Where(s => s.FamilyName.StartsWith("Finca ")).GroupBy(s => s.FamilyName).ToDictionary(g => g.Key, g => g.First());
foreach (var s in syms.Values) if (!s.IsActive) s.Activate();
int n = 0;
foreach (var ln in System.IO.File.ReadAllLines("${f}").Skip(${i}).Take(${CHUNK})) {
  var p = ln.Split('|'); var pt = new XYZ(double.Parse(p[1]) / 304.8, double.Parse(p[2]) / 304.8, double.Parse(p[3]) / 304.8);
  var fi = document.Create.NewFamilyInstance(pt, syms[p[0]], lvl, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
  ElementTransformUtils.RotateElement(document, fi.Id, Line.CreateBound(pt, pt + XYZ.BasisZ), (n * 137 % 360) * Math.PI / 180);
  n++;
}
return n;`);
    total += Number(r); console.log('  colocados', total, '/', T.length);
  }
  console.log('vegetacion: ' + total);
})().catch(e => { console.error('ERROR', e.message); process.exit(1); });
