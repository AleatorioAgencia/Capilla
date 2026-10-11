// Etapas de realismo para el modelo de la finca. Uso: node realismo.cjs <etapa>
// Genera C# y lo ejecuta en Revit con send_code_to_revit (cuerpo de Execute(document, parameters, uiapp)).
// El plugin solo lee 8 KB por petición: los datos van en archivos de texto (scripts/data/*.txt) que el C# lee de disco.
const net = require('net'), fs = require('fs'), path = require('path');
const finca = require('./finca.cjs');
const ids = require('./finca_ids.json');
const DATA = path.join(__dirname, 'data'); fs.mkdirSync(DATA, { recursive: true });
const dataFile = (name, lines) => { const f = path.join(DATA, name); fs.writeFileSync(f, lines.join('\n')); return f.replace(/\\/g, '\\\\'); };

function runCode(code, transactionMode) {
  const bytes = Buffer.byteLength(code);
  if (bytes > 7500) return Promise.reject(new Error('codigo demasiado largo: ' + bytes + ' bytes'));
  return new Promise((res, rej) => {
    const params = { code, parameters: [] }; if (transactionMode) params.transactionMode = transactionMode;
    const s = net.connect(8080, '127.0.0.1', () => s.write(JSON.stringify({ jsonrpc: '2.0', method: 'send_code_to_revit', params, id: String(Date.now()) })));
    let b = ''; s.on('data', d => { b += d; try { const r = JSON.parse(b); s.end(); const x = r.result || r.error; x.success ? res(x.result) : rej(new Error(x.errorMessage || x.message)); } catch { } });
    s.on('error', rej); setTimeout(() => rej(new Error('timeout')), 300000);
  });
}

// Materiales: nombre | asset de la biblioteca | color de sombreado R,G,B
const MATS = [
  ['Finca - Cesped', 'Prism-840', '92,140,52'],
  ['Finca - Concreto anden', 'Concrete-017', '175,172,165'],
  ['Finca - Tierra camino', 'Generic-020', '140,110,80'],
  ['Finca - Agua', 'Prism-236', '60,95,70'],
  ['Finca - Estuco crema', 'Prism-814', '228,218,190'],
  ['Finca - Blanco', 'Concrete-019', '240,240,236'],
  ['Finca - Lamina verde', 'Prism-098', '90,130,105'],
  ['Finca - Teja roja', 'Prism-2058', '150,60,45'],
  ['Finca - Lamina gris', 'Prism-2056', '130,132,135'],
  ['Finca - Follaje', 'Paint-037', '55,95,40'],
  ['Finca - Follaje claro', 'Paint-038', '150,170,90'],
  ['Finca - Buganvilla', 'Prism-091', '190,50,150'],
  ['Finca - Tronco', 'Wood-010', '90,65,45'],
];

// Fragmento C#: crea/actualiza materiales desde data/mats.txt -> diccionario M
const matCode = f => `
var M = new Dictionary<string, ElementId>();
var lib = uiapp.Application.GetAssets(Autodesk.Revit.DB.Visual.AssetType.Appearance);
foreach (var ln in System.IO.File.ReadAllLines("${f}")) {
  var p = ln.Split('|'); var c = p[2].Split(',').Select(byte.Parse).ToArray();
  var m = new FilteredElementCollector(document).OfClass(typeof(Material)).Cast<Material>().FirstOrDefault(x => x.Name == p[0]);
  if (m == null) m = document.GetElement(Material.Create(document, p[0])) as Material;
  m.Color = new Color(c[0], c[1], c[2]);
  if (m.AppearanceAssetId == ElementId.InvalidElementId) {
    var a = lib.FirstOrDefault(x => x.Name == p[1]);
    if (a != null) {
      var an = p[0] + " (aspecto)";
      var ae = new FilteredElementCollector(document).OfClass(typeof(AppearanceAssetElement)).Cast<AppearanceAssetElement>().FirstOrDefault(x => x.Name == an)
               ?? AppearanceAssetElement.Create(document, an, a);
      m.AppearanceAssetId = ae.Id;
    }
  }
  M[p[0]] = m.Id;
}`;

// Fragmento C#: tipo duplicado con todas sus capas en un material
const typeCode = `
T Dup<T>(long baseId, string name, ElementId mat) where T : HostObjAttributes {
  var t = new FilteredElementCollector(document).OfClass(typeof(T)).Cast<T>().FirstOrDefault(x => x.Name == name);
  if (t == null) t = (T)((T)document.GetElement(new ElementId(baseId))).Duplicate(name);
  var cs = t.GetCompoundStructure();
  if (cs != null) { for (int i = 0; i < cs.LayerCount; i++) cs.SetMaterialId(i, mat); t.SetCompoundStructure(cs); }
  return t;
}`;

function floorMat(name) {
  if (/Terreno/.test(name)) return 'Finca - Cesped';
  if (/Estanque/.test(name)) return 'Finca - Agua';
  if (/Camino de tierra|Explanada/.test(name)) return 'Finca - Tierra camino';
  if (/Jardinera|Tanque|Santuario/.test(name)) return 'Finca - Blanco';
  return 'Finca - Concreto anden';
}
const wallMat = name => /Casa principal/.test(name) ? 'Finca - Estuco crema' : 'Finca - Blanco';

const etapas = {
  // 1. Materiales y asignación a suelos y muros
  async materiales() {
    const fm = dataFile('mats.txt', MATS.map(m => m.join('|')));
    // id | categoria (F/W) | material
    const fe = dataFile('elem_mats.txt', [
      ...finca.floors.map((f, i) => `${ids.floors[i]}|F|${floorMat(f.name)}`),
      ...finca.walls.map((w, i) => `${ids.walls[i]}|W|${wallMat(w.name)}`),
    ]);
    return runCode(`${matCode(fm)}${typeCode}
int n = 0;
foreach (var ln in System.IO.File.ReadAllLines("${fe}")) {
  var p = ln.Split('|'); var e = document.GetElement(new ElementId(long.Parse(p[0]))); if (e == null) continue;
  var short_ = p[2].Replace("Finca - ", "");
  var t = p[1] == "F" ? (ElementType)Dup<FloorType>(${finca.T.suelo}, "Finca suelo - " + short_, M[p[2]])
                      : Dup<WallType>(${finca.T.muro}, "Finca muro - " + short_, M[p[2]]);
  if (e.GetTypeId() != t.Id) e.ChangeTypeId(t.Id); n++;
}
return "materiales: " + M.Count + ", elementos actualizados: " + n;`);
  },
};

module.exports = { runCode, dataFile, matCode, typeCode, MATS };
if (require.main === module) {
  const etapa = process.argv[2];
  if (!etapas[etapa]) { console.log('etapas:', Object.keys(etapas).join(', ')); process.exit(1); }
  etapas[etapa]().then(r => console.log(r), e => { console.error('ERROR', e.message); process.exit(1); });
}
