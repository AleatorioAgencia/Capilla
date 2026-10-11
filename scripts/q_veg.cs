var sb = new System.Text.StringBuilder();
var v = uiapp.ActiveUIDocument.ActiveView;
sb.AppendLine("vista activa: " + v.Name + " estilo=" + v.DisplayStyle);
var inst = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().Where(i => i.Symbol.FamilyName.StartsWith("Finca ")).ToList();
sb.AppendLine("instancias Finca: " + inst.Count);
if (inst.Count > 0) { var i0 = inst[0]; sb.AppendLine("categoria: " + i0.Category.Name + " | oculta en vista: " + v.GetCategoryHidden(i0.Category.Id) + " | loc z(m): " + (((LocationPoint)i0.Location).Point.Z * 0.3048).ToString("0.00"));
  var bb = i0.get_BoundingBox(null); sb.AppendLine("bbox alto(m): " + (bb == null ? "null" : ((bb.Max.Z - bb.Min.Z) * 0.3048).ToString("0.0"))); }
var t = new FilteredElementCollector(document, v.Id).OfCategory(BuiltInCategory.OST_Planting).GetElementCount();
sb.AppendLine("plantas visibles en vista: " + t);
var fl = new FilteredElementCollector(document).OfClass(typeof(Floor)).Cast<Floor>().Where(f => f.Name.Contains("Cesped")).Select(f => f.Name).Distinct();
sb.AppendLine("suelos cesped: " + string.Join(",", fl));
uiapp.ActiveUIDocument.RefreshActiveView();
return sb.ToString();
