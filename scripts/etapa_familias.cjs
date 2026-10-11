// Etapa 4a: crea familias de vegetación (modelo genérico -> categoría Plantas) y las carga en el proyecto.
// Formas: cyl:r,z0,z1,mat  ell:rx,rz,zc,mat  cone:r,z0,z1,mat  (metros)
const { runCode, dataFile } = require('./realismo.cjs');
const FAM = {
  'Finca Arbol frondoso': 'cyl:0.2,0,3,Tronco;ell:3.5,3,6,Follaje',
  'Finca Arbol claro': 'cyl:0.15,0,2,Tronco;ell:2.6,2.3,4.3,Follaje claro',
  'Finca Pino': 'cyl:0.25,0,17,Tronco;cone:3.2,2,8,Follaje;cone:2.5,6,12,Follaje;cone:1.6,10,15.5,Follaje;cone:0.9,14,18,Follaje',
  'Finca Cipres': 'cyl:0.12,0,0.8,Tronco;ell:0.7,3,3.6,Follaje',
  'Finca Palma': 'cyl:0.15,0,5,Tronco;ell:2.6,0.8,5.3,Follaje',
  'Finca Arbusto': 'ell:1.1,0.8,0.6,Follaje',
  'Finca Buganvilla': 'ell:1.4,1.1,1,Buganvilla',
  'Finca Arbol bosque': 'cyl:0.4,0,9,Tronco;ell:6,6,13,Follaje',
};
const dir = require('path').resolve(__dirname, '../revit/familias').split('\\').join('/');
const f = dataFile('familias.txt', Object.entries(FAM).map(([n, s]) => n + '|' + s));
runCode(`var app = uiapp.Application; var outp = new List<string>();
var tpl = System.IO.Directory.GetFiles("C:/ProgramData/Autodesk/RVT 2027/Family Templates/Spanish", "Modelo gen*rico m*trico.rft").First(x => System.IO.Path.GetFileName(x).Length < 30);
Func<double, double> U = v => v / 0.3048;
using (var t0 = new Transaction(document, "borrar familias Finca")) { t0.Start(); foreach (var fam0 in new FilteredElementCollector(document).OfClass(typeof(Family)).Cast<Family>().Where(x => x.Name.StartsWith("Finca ")).Select(x => x.Id).ToList()) document.Delete(fam0); t0.Commit(); }
foreach (var ln in System.IO.File.ReadAllLines("${f}")) {
  var parts = ln.Split('|'); var name = parts[0]; var path = "${dir}/" + name + ".rfa";
  var fd = app.NewFamilyDocument(tpl);
  using (var t = new Transaction(fd, "veg")) {
    t.Start();
    try { fd.OwnerFamily.FamilyCategory = fd.Settings.Categories.get_Item(BuiltInCategory.OST_Planting); } catch { }
    var mats = new Dictionary<string, ElementId>();
    ElementId Mat(string m) { if (!mats.ContainsKey(m)) mats[m] = Material.Create(fd, "Finca - " + m); return mats[m]; }
    var spV = SketchPlane.Create(fd, Plane.CreateByNormalAndOrigin(XYZ.BasisY, XYZ.Zero));
    var axis = Line.CreateBound(XYZ.Zero, XYZ.BasisZ);
    foreach (var sh in parts[1].Split(';')) {
      var k = sh.Split(':'); var a = k[1].Split(','); double p0 = U(double.Parse(a[0])), p1 = U(double.Parse(a[1])), p2 = U(double.Parse(a[2]));
      var ca = new CurveArray(); GenericForm g;
      if (k[0] == "cyl") {
        var pl = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, p1));
        ca.Append(Arc.Create(pl, p0, 0, Math.PI)); ca.Append(Arc.Create(pl, p0, Math.PI, 2 * Math.PI));
        var caa = new CurveArrArray(); caa.Append(ca);
        g = fd.FamilyCreate.NewExtrusion(true, caa, SketchPlane.Create(fd, pl), p2 - p1);
      } else {
        if (k[0] == "ell") {
          var c = new XYZ(0, 0, p2);
          ca.Append(Ellipse.CreateCurve(c, p0, p1, XYZ.BasisX, XYZ.BasisZ, -Math.PI / 2, Math.PI / 2));
          ca.Append(Line.CreateBound(new XYZ(0, 0, p2 + p1), new XYZ(0, 0, p2 - p1)));
        } else {
          ca.Append(Line.CreateBound(new XYZ(0, 0, p1), new XYZ(p0, 0, p1)));
          ca.Append(Line.CreateBound(new XYZ(p0, 0, p1), new XYZ(0, 0, p2)));
          ca.Append(Line.CreateBound(new XYZ(0, 0, p2), new XYZ(0, 0, p1)));
        }
        var caa = new CurveArrArray(); caa.Append(ca);
        g = fd.FamilyCreate.NewRevolution(true, caa, spV, axis, 0, 2 * Math.PI);
      }
      g.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM).Set(Mat(a[3]));
    }
    t.Commit();
  }
  fd.SaveAs(path, new SaveAsOptions { OverwriteExistingFile = true }); fd.Close(false);
  using (var t = new Transaction(document, "cargar " + name)) {
    t.Start(); Family fam; document.LoadFamily(path, out fam); t.Commit();
  }
  outp.Add(name);
}
return "familias: " + string.Join(", ", outp);`, 'none').then(console.log, e => { console.error('ERROR', e.message); process.exit(1); });
