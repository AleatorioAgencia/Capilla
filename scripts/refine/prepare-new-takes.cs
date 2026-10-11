var dest=@"C:\Users\admin\.codex\.chatgpt-projects\g-p-6acadee8e81c8191910d9dd6cf16b614\Capilla\revit\Capilla_ajuste_nuevas_tomas.rvt";
if(!document.PathName.EndsWith("Capilla_conjunto_corregido.rvt")&&document.PathName!=dest)throw new Exception("Documento inesperado");
if(document.PathName!=dest){if(System.IO.File.Exists(dest))throw new Exception("Destino ya existente");document.SaveAs(dest,new SaveAsOptions{Compact=true,MaximumBackups=2});}
return new{path=document.PathName,saved=!document.IsModified};
