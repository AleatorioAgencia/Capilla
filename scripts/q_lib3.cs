var sb=new System.Text.StringBuilder(); int k=0;
foreach(var a in uiapp.Application.GetAssets(Autodesk.Revit.DB.Visual.AssetType.Appearance)){
  if(k++>2) break;
  for(int i=0;i<a.Size;i++){ var p=a.Get(i); var s=p as Autodesk.Revit.DB.Visual.AssetPropertyString; sb.AppendLine(a.Name+" . "+p.Name+" ["+p.Type+"] "+(s!=null?s.Value:"")); }
}
return sb.ToString();
