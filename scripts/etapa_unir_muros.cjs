// Etapa 2b: une la parte superior de los muros de cada edificio a su cubierta (cierra los hastiales).
const { runCode, dataFile } = require('./realismo.cjs');
const finca = require('./finca.cjs'), ids = require('./finca_ids.json');
const roofByName = Object.fromEntries(finca.roofs.map((r, i) => [r.name.replace(' - cubierta', ''), ids.roofs[i]]));
const lines = finca.walls.map((w, i) => roofByName[w.name] ? `${ids.walls[i]}|${roofByName[w.name]}` : null).filter(Boolean);
const f = dataFile('wall_roof.txt', lines);
runCode(`int n = 0; var errs = new List<string>();
foreach (var ln in System.IO.File.ReadAllLines("${f}")) {
  var p = ln.Split('|'); var w = document.GetElement(new ElementId(long.Parse(p[0]))) as Wall; if (w == null) continue;
  try { w.AddAttachment(new ElementId(long.Parse(p[1])), AttachmentLocation.Top); n++; } catch (Exception ex) { errs.Add(ex.Message); }
}
return "muros unidos: " + n + (errs.Count > 0 ? " | errores: " + string.Join("; ", errs.Distinct()) : "");`).then(console.log, e => { console.error('ERROR', e.message); process.exit(1); });
