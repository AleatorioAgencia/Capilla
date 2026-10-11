// Etapa 5: baranda del corredor de la casa principal + vista 3D realista con ángulo de dron.
const { runCode } = require('./realismo.cjs');
runCode(`var outp = new List<string>(); double F = 304.8;
var lvl = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().First(l => Math.Abs(l.Elevation) < 0.001);
// --- baranda del corredor (frente y laterales, hueco central para la entrada) ---
var rtype = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Architecture.RailingType)).FirstElement();
if (rtype != null) {
  var tramos = new[] { new[] { -9.0, -4.5, -9.0, -7.4, -1.0, -7.4 }, new[] { 1.0, -7.4, 9.0, -7.4, 9.0, -4.5 } };
  foreach (var tr in tramos) {
    var cl = new CurveLoop();
    cl.Append(Line.CreateBound(new XYZ(tr[0] * 1000 / F, tr[1] * 1000 / F, 0), new XYZ(tr[2] * 1000 / F, tr[3] * 1000 / F, 0)));
    cl.Append(Line.CreateBound(new XYZ(tr[2] * 1000 / F, tr[3] * 1000 / F, 0), new XYZ(tr[4] * 1000 / F, tr[5] * 1000 / F, 0)));
    var r = Autodesk.Revit.DB.Architecture.Railing.Create(document, cl, rtype.Id, lvl.Id);
    var off = r.get_Parameter(BuiltInParameter.STAIRS_RAILING_HEIGHT_OFFSET); if (off != null && !off.IsReadOnly) off.Set(200 / F);
  }
  outp.Add("barandas: 2 (" + rtype.Name + ")");
}
// --- vista 3D ---
var v = new FilteredElementCollector(document).OfClass(typeof(View3D)).Cast<View3D>().FirstOrDefault(x => !x.IsTemplate && x.Name == "{3D}");
if (v != null) {
  v.DisplayStyle = DisplayStyle.Realistic;
  v.DetailLevel = ViewDetailLevel.Fine;
  try { v.CropBoxActive = false; } catch { }
  var eye = new XYZ(-70000 / F, -165000 / F, 75000 / F); var target = new XYZ(0, -5000 / F, 0);
  var fwd = (target - eye).Normalize(); var right = fwd.CrossProduct(XYZ.BasisZ).Normalize(); var up = right.CrossProduct(fwd);
  v.SetOrientation(new ViewOrientation3D(eye, up, fwd));
  foreach (var bic in new[] { BuiltInCategory.OST_Levels, BuiltInCategory.OST_Grids, BuiltInCategory.OST_Elev, BuiltInCategory.OST_Sections })
    try { v.SetCategoryHidden(new ElementId(bic), true); } catch { }
  try { var sh = v.get_Parameter(BuiltInParameter.MODEL_GRAPHICS_STYLE); } catch { }
  outp.Add("vista {3D}: Realista, detalle alto, angulo de dron");
}
return string.Join(" | ", outp);`).then(console.log, e => { console.error('ERROR', e.message); process.exit(1); });
