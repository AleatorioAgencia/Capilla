var sb=new System.Text.StringBuilder();
var assets=uiapp.Application.GetAssets(Autodesk.Revit.DB.Visual.AssetType.Appearance);
sb.AppendLine("assets: "+assets.Count);
foreach(var a in assets){ var n=a.Name+" | "+a.Title; if(System.Text.RegularExpressions.Regex.IsMatch(n,"(?i)grass|c[eé]sped|hierba|soil|dirt|tierra|gravel|grava|water|agua|pool|concrete|hormig|stucco|enlucido|plaster|metal.*(paint|green)|corrugated|ondulad|terracota|clay|teja|roof|wood|madera|cubierta|lawn|pavement|asfalto")) sb.AppendLine(a.Name+" | "+a.Title); }
return sb.ToString();
