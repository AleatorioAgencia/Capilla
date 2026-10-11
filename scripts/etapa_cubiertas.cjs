// Etapa 2: sustituye las cubiertas planas por cubiertas inclinadas (4 aguas / 2 aguas) con material.
const { runCode, dataFile, matCode, typeCode, MATS } = require('./realismo.cjs');
const finca = require('./finca.cjs'), ids = require('./finca_ids.json'), fs = require('fs');
const cfg = n => /Casa principal/.test(n) ? ['hip', 'Finca - Lamina verde', 20]
  : /Santuario/.test(n) ? ['hip', 'Finca - Teja roja', 35]
  : /Portada/.test(n) ? ['gable', 'Finca - Teja roja', 30]
  : /Invernadero/.test(n) ? ['gable', 'Finca - Blanco', 25]
  : /izquierda/.test(n) ? ['gable', 'Finca - Lamina gris', 22]
  : ['gable', 'Finca - Lamina gris', 20];
const lines = finca.roofs.map((r, i) => {
  const xs = r.boundary.outerLoop.map(s => s.p0.x), ys = r.boundary.outerLoop.map(s => s.p0.y);
  const [kind, mat, deg] = cfg(r.name);
  return [ids.roofs[i], Math.min(...xs), Math.min(...ys), Math.max(...xs), Math.max(...ys), r.baseOffset, kind, mat, deg].join('|');
});
const fm = dataFile('mats.txt', MATS.map(m => m.join('|')));
const fr = dataFile('roofs.txt', lines);
runCode(`${matCode(fm)}${typeCode}
var lvl = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().First(l => Math.Abs(l.Elevation) < 0.001);
var outIds = new List<string>(); double F = 304.8;
var st = new SubTransaction(document); st.Start();
try {
foreach (var ln in System.IO.File.ReadAllLines("${fr}")) {
  var p = ln.Split('|'); double x0 = double.Parse(p[1]) / F, y0 = double.Parse(p[2]) / F, x1 = double.Parse(p[3]) / F, y1 = double.Parse(p[4]) / F, h = double.Parse(p[5]) / F;
  var old = document.GetElement(new ElementId(long.Parse(p[0]))); if (old != null) document.Delete(old.Id);
  var rt = Dup<RoofType>(1400, "Finca cubierta - " + p[7].Replace("Finca - ", ""), M[p[7]]);
  var pts = new[] { new XYZ(x0, y0, 0), new XYZ(x1, y0, 0), new XYZ(x1, y1, 0), new XYZ(x0, y1, 0) };
  var ca = new CurveArray(); for (int i = 0; i < 4; i++) ca.Append(Line.CreateBound(pts[i], pts[(i + 1) % 4]));
  var mca = new ModelCurveArray(); var roof = document.Create.NewFootPrintRoof(ca, lvl, rt, out mca);
  roof.get_Parameter(BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM).Set(h);
  bool alongX = (x1 - x0) >= (y1 - y0); double slope = Math.Tan(double.Parse(p[8]) * Math.PI / 180);
  foreach (ModelCurve mc in mca) {
    var d = ((Line)mc.GeometryCurve).Direction; bool parallelX = Math.Abs(d.X) > 0.5;
    bool defines = p[6] == "hip" || (alongX ? parallelX : !parallelX);
    roof.set_DefinesSlope(mc, defines); if (defines) roof.set_SlopeAngle(mc, slope);
  }
  outIds.Add(roof.Id.Value.ToString());
}
st.Commit();
} catch (Exception ex) { st.RollBack(); return "ERR " + ex.GetType().Name + ": " + ex.Message + " @ " + ex.StackTrace; }
return string.Join(",", outIds);`).then(r => {
  const o = require('./finca_ids.json'); if (JSON.parse(r).startsWith('ERR')) throw new Error(JSON.parse(r)); o.roofs = JSON.parse(r).split(',').map(Number);
  fs.writeFileSync(__dirname + '/finca_ids.json', JSON.stringify(o, null, 1)); console.log('cubiertas nuevas:', o.roofs.length);
}, e => { console.error('ERROR', e.message); process.exit(1); });
