var sb=new System.Text.StringBuilder();
string S(Autodesk.Revit.DB.Visual.Asset a,string n){var p=a.FindByName(n) as Autodesk.Revit.DB.Visual.AssetPropertyString;return p==null?"":p.Value;}
foreach(var a in uiapp.Application.GetAssets(Autodesk.Revit.DB.Visual.AssetType.Appearance)){
  var d=S(a,"description")+" | "+S(a,"keyword");
  if(System.Text.RegularExpressions.Regex.IsMatch(d,"(?i)c[eé]sped|hierba|grass|tierra|grava|agua|estuco|enlucido|ondulad|met[aá]lica.*cubierta|cubierta.*met|teja|tejado|pintura.*verde|hormig[oó]n.*(liso|cepillado|acera)|corteza|follaje|hoja")) sb.AppendLine(a.Name+" | "+S(a,"BaseSchema")+" | "+d);
}
return sb.ToString();
