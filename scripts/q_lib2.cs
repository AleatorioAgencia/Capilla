var sb=new System.Text.StringBuilder();
foreach(var a in uiapp.Application.GetAssets(Autodesk.Revit.DB.Visual.AssetType.Appearance)){
  var p=a.FindByName("UIName") as Autodesk.Revit.DB.Visual.AssetPropertyString; var n=p!=null?p.Value:"";
  if(System.Text.RegularExpressions.Regex.IsMatch(n,"(?i)grass|c[eé]sped|hierba|soil|dirt|tierra|gravel|grava|water|agua|stucco|enlucido|plaster|corrugat|ondulad|terra ?cott|teja|shingle|lawn|pavement|sidewalk|acera|pavimento|piedra|stone|pintura.*verde|green")) sb.AppendLine(a.Name+" = "+n);
}
return sb.ToString();
