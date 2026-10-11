var sb=new System.Text.StringBuilder();
sb.AppendLine("Revit "+uiapp.Application.VersionNumber+" | vista: "+uiapp.ActiveUIDocument.ActiveView.Name);
foreach(Level l in new FilteredElementCollector(document).OfClass(typeof(Level))) sb.AppendLine("LEVEL "+l.Id+" "+l.Name+" "+(l.Elevation*304.8));
sb.AppendLine("Muros: "+new FilteredElementCollector(document).OfClass(typeof(Wall)).GetElementCount());
foreach(ToposolidType t in new FilteredElementCollector(document).OfClass(typeof(ToposolidType))) sb.AppendLine("TOPO "+t.Id+" "+t.Name);
foreach(Material m in new FilteredElementCollector(document).OfClass(typeof(Material))) if(System.Text.RegularExpressions.Regex.IsMatch(m.Name,"(?i)c[eé]sped|hierba|grass|tierra|agua|water|hormig|concret|grava|teja|metal|madera|ladrillo|enlucido|pintura|vidrio")) sb.AppendLine("MAT "+m.Id+" "+m.Name+" asset="+m.AppearanceAssetId);
return sb.ToString();
