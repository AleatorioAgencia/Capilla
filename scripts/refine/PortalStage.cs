 static void SitePortal(){Tx("Portada de acceso segun recorrido 08m02",()=>{
  foreach(long id in new long[]{154779,154911,154912,154913,154914,155456})Del(id);
  foreach(var e in new FilteredElementCollector(D).WhereElementIsNotElementType().Where(e=>(e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").StartsWith("SITIO_9399 - Portada")).ToList())D.Delete(e.Id);
  RemoveShape("SITIO - Portada pilares y muros");RemoveShape("SITIO - Portada tejas y cerchas");
  SiteMat("Ladrillo portada",175,145,110);SiteMat("Junta portada",214,202,176);
  double x=-38,y=-34.6,z=Height(x,y)+.16;
  SiteRoof("Portada - tejado sobre paso vehicular",x,y,2.9,6.6,z+3.66,.30,false,"Teja");
  var walls=new List<GeometryObject>();var detail=new List<GeometryObject>();
  foreach(double cy in new[]{y-2.5,y+2.5}){
   walls.Add(Box(x-.34,cy-.34,z-.10,.68,.68,3.38,"Ladrillo portada"));
   walls.Add(Box(x-.40,cy-.40,z-.10,.8,.8,.16,"Concreto"));
   for(int row=1;row<44;row++){double h=row*.075;
    walls.Add(Box(x-.343,cy-.343,z+h,.686,.006,.008,"Junta portada"));
    walls.Add(Box(x-.343,cy+.337,z+h,.686,.006,.008,"Junta portada"));
    walls.Add(Box(x-.343,cy-.337,z+h,.006,.674,.008,"Junta portada"));
    walls.Add(Box(x+.337,cy-.337,z+h,.006,.674,.008,"Junta portada"));
    foreach(double s in row%2==0?new[]{-.22,.06}:new[]{-.08,.20}){
     walls.Add(Box(x-.344,cy+s,z+h,.006,.008,.067,"Junta portada"));
     walls.Add(Box(x+.338,cy+s,z+h,.006,.008,.067,"Junta portada"));
    }
   }
   detail.Add(Rod(P(x-1.3,cy,z+3.66),P(x+1.3,cy,z+3.66),.045,"Madera oscura",4));
   detail.Add(Rod(P(x-.3,cy,z+3.28),P(x-1.1,cy,z+3.66),.022,"Carpinteria",4));
   detail.Add(Rod(P(x+.3,cy,z+3.28),P(x+1.1,cy,z+3.66),.022,"Carpinteria",4));
  }
  // Stepped side wings seen at 08:02; lengths and heights remain approximate.
  foreach(double side in new[]{-1d,1d})for(int n=0;n<3;n++){
   double cy=y+side*(3.28+n*.72),h=1.65-n*.25;
   walls.Add(Box(x-.14,cy-.36,z-.14,.28,.72,h+.14,"Ladrillo portada"));
   walls.Add(Box(x-.19,cy-.39,z+h,.38,.78,.065,"Concreto"));
   for(double zz=.075;zz<h;zz+=.075)foreach(double xx in new[]{x-.144,x+.138})walls.Add(Box(xx,cy-.36,z+zz,.006,.72,.008,"Junta portada"));
  }
  for(double yy=y-3.26;yy<y+3.27;yy+=.20){
   foreach(double side in new[]{-1d,1d})detail.Add(Rod(P(x+side*1.46,yy,z+3.805),P(x,yy,z+3.805+1.46*Math.Tan(.30)),.05,"Teja",8));
  }
  detail.Add(Rod(P(x,y-3.3,z+4.29),P(x,y+3.3,z+4.29),.105,"Teja",12));
  Shape("SITIO - Portada pilares y muros",walls);Shape("SITIO - Portada tejas y cerchas",detail);
  Camera("CAPILLA - 11 Portada de acceso",C(-49,-47,4),C(-38,-34,-2),C(-45,-42,-8),C(-31,-27,4));
  var v=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().First(v=>v.Name=="CAPILLA - 11 Portada de acceso");
  var t=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().First(v=>v.Name=="CAPILLA - Presentacion con sombras");v.ApplyViewTemplateParameters(t);
 });}
