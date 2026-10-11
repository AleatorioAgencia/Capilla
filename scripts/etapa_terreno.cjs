// Etapa 3: toposólido alrededor de la finca (plano bajo la finca, ladera hacia el frente, montañas al fondo).
const { runCode, dataFile, matCode, typeCode, MATS } = require('./realismo.cjs');
const { h, X0, X1, Y0, Y1, BASE } = require('./terreno_h.cjs');
const pts = [];
for (let x = -170; x <= 175; x += 10) for (let y = -150; y <= 200; y += 10) pts.push([x, y, h(x, y)]);
for (let x = X0; x <= X1; x += 5) pts.push([x, Y0, BASE], [x, Y1, BASE]);
for (let y = Y0; y <= Y1; y += 5) pts.push([X0, y, BASE], [X1, y, BASE]);
const uniq = [...new Map(pts.map(p => [p[0] + ',' + p[1], p])).values()];
const f = dataFile('topo.txt', uniq.map(p => p.map(v => Math.round(v * 1000)).join(',')));
const fm = dataFile('mats.txt', MATS.map(m => m.join('|')));
runCode(`${matCode(fm)}${typeCode}
foreach (var old in new FilteredElementCollector(document).OfClass(typeof(Toposolid)).ToElementIds().ToList()) document.Delete(old);
var lvl = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().First(l => Math.Abs(l.Elevation) < 0.001);
var baseType = new FilteredElementCollector(document).OfClass(typeof(ToposolidType)).First();
var tt = Dup<ToposolidType>(baseType.Id.Value, "Finca - Terreno cesped", M["Finca - Cesped"]);
var pts = System.IO.File.ReadAllLines("${f}").Select(l => l.Split(',').Select(v => double.Parse(v) / 304.8).ToArray()).Select(a => new XYZ(a[0], a[1], a[2])).ToList();
var topo = Toposolid.Create(document, pts, tt.Id, lvl.Id);
return "toposolido " + topo.Id + " con " + pts.Count + " puntos";`).then(console.log, e => { console.error('ERROR', e.message); process.exit(1); });
