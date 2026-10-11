 static double[] NC={32.94761590338957,51.664271694742816};
 static double[] NT={27.58629496291247,65.6193581945095};
 static double[] NS={-7.118855623385877,36.06138757408541};
 static double[] NK={1.0,36.0};
 static double[] NL={-50.24373418343728,-5.938301791669189};
 static double[] NI={-24.576322425986476,-61.09194449518144};
 static double[][] NP={new[]{9d,26,5,4.5},new[]{-1.844400263977052,21.956044987487793,3.4,5},new[]{2d,-50,5.6,3.8}};
 static double[][] RearRoute={new[]{0d,10.73,1.73},new[]{0d,12,1.77},new[]{-2.6,15.1,1.94},new[]{-7.4,17.7,2.2},new[]{-8.2,22,2.55},new[]{-8d,28,3.35},new[]{-13.3,32.2,3.90},new[]{-14.2,39.45,5.17}};
 static double[][] DriveRoute={new[]{15d,-24},new[]{21d,-15},new[]{26d,-3},new[]{32d,10}};
 static void NMark(Element e,string n){e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.Set("NUEVAS_1210 - "+n+" - posiciones del propietario y videos 12-10-2026; dimensiones estimadas");}
 static void NRemove(string prefix){foreach(var e in new FilteredElementCollector(D).WhereElementIsNotElementType().Where(e=>(e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").StartsWith("NUEVAS_1210 - "+prefix)).ToList())if(e.IsValidObject&&e.Id.Value!=154763)D.Delete(e.Id);foreach(var e in new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(e=>e.ApplicationDataId.StartsWith("1210 "+prefix)).ToList())if(e.IsValidObject)D.Delete(e.Id);}
 static void NAt(long id,double x,double y,double dz=0){var e=D.GetElement(E(id));if(e==null)return;var b=e.get_BoundingBox(null);var c=(b.Min+b.Max)*.1524;var delta=P(x-c.X,y-c.Y,dz);if(delta.GetLength()>1e-7)ElementTransformUtils.MoveElement(D,e.Id,delta);}
 static void NM(long id,double dx,double dy,double dz=0){if(D.GetElement(E(id))!=null)ElementTransformUtils.MoveElement(D,E(id),P(dx,dy,dz));}
 static double NLvl(int j){return j==2?Base(2,-50)-.4:Base(NP[j][0],NP[j][1])+.25;}
 static double[] NPoint(int j,double a,double scale=1){var q=NP[j];double xx,yy;if(j==2){xx=Math.Sign(Math.Cos(a))*Math.Pow(Math.Abs(Math.Cos(a)),.45);yy=Math.Sign(Math.Sin(a))*Math.Pow(Math.Abs(Math.Sin(a)),.45);}else{double r=j==1?1+.18*Math.Cos(3*a):1;xx=r*Math.Cos(a);yy=r*Math.Sin(a);}return new[]{q[0]+q[2]*xx*scale,q[1]+q[3]*yy*scale};}
 static double NRaw(double x,double y){double z=Base(x,y)+.24*Math.Sin(x/11)*Math.Sin(y/15);var pads=new[]{new[]{0d,-.7,20,13.8,-.17},new[]{48d,18,11.7,9.5,Base(48,18)-.17},new[]{NL[0],NL[1],11d,9,Base(NL[0],NL[1])-.17},new[]{-50d,12,8,7,Base(-50,12)-.17},new[]{NS[0],NS[1],8.0,5.0,Base(NS[0],NS[1])-.17},new[]{NK[0],NK[1],6.1,6.1,Base(NK[0],NK[1])-.17},new[]{NC[0],NC[1],6.6,15d,Base(NC[0],NC[1])-.17},new[]{NT[0],NT[1],7.2,5.6,Base(NT[0],NT[1])-.17},new[]{26d,30,4.8,4.1,Base(26,30)-.17},new[]{NI[0],NI[1],11d,11,Base(NI[0],NI[1])-.17},new[]{-15d,-48,6.5,6.5,Height(-15,-48)-.1},new[]{-14.2,40.3,2d,2d,5.0}};
  foreach(var p in pads){double dx=Math.Max(Math.Abs(x-p[0])-p[2]/2,0),dy=Math.Max(Math.Abs(y-p[1])-p[3]/2,0),d=Math.Sqrt(dx*dx+dy*dy);if(d<4){double t=d/4;t=t*t*(3-2*t);z=p[4]*(1-t)+z*t;}}
  for(int j=0;j<3;j++){var q=NP[j];double xx=(x-q[0])/q[2],yy=(y-q[1])/q[3];double a=Math.Atan2(yy,xx);double r=j==2?Math.Pow(Math.Pow(Math.Abs(xx),4.4)+Math.Pow(Math.Abs(yy),4.4),1/4.4):Math.Sqrt(xx*xx+yy*yy)/(j==1?1+.18*Math.Cos(3*a):1);double level=NLvl(j);if(r<.98)z=level-.9;else if(r<1.40){double t=Math.Max(0,Math.Min(1,(r-.98)/.08));z=(level-.9)*(1-t)+(level-.35)*t;}else if(r<1.9){double t=(r-1.4)/.5;t=t*t*(3-2*t);z=(level-.35)*(1-t)+z*t;}}
  return z;
 }
 static double NHeight(double x,double y){double z=NRaw(x,y),best=1e9,ground=z;for(int i=0;i<RearRoute.Length-1;i++){var a=RearRoute[i];var b=RearRoute[i+1];double dx=b[0]-a[0],dy=b[1]-a[1],t=Math.Max(0,Math.Min(1,((x-a[0])*dx+(y-a[1])*dy)/(dx*dx+dy*dy)));double d=Math.Sqrt(Math.Pow(x-a[0]-t*dx,2)+Math.Pow(y-a[1]-t*dy,2));if(d<best){best=d;ground=a[2]+t*(b[2]-a[2])-.17;}}if(best<2.0){double t=Math.Max(0,(best-.8)/1.2);t=t*t*(3-2*t);z=ground*(1-t)+z*t;}if(y>=6.55&&y<=10.73&&Math.Abs(x)<2){double gz=.08+(y-6.55)/4.18*1.65-.18,t=Math.Max(0,(Math.Abs(x)-.8)/1.2);t=t*t*(3-2*t);z=gz*(1-t)+z*t;}return z;}
 static void NLayout(){Tx("Alinear piezas respetando movimientos del propietario",()=>{
  double dz=Base(NC[0],NC[1])-Base(55,50);NAt(165657,NC[0],NC[1],dz);NAt(165666,NC[0],NC[1],dz);NAt(165712,NC[0]+.01,NC[1],dz);
  dz=Base(NT[0],NT[1])-Base(58,64);NAt(165715,NT[0],NT[1],dz);NM(165720,0,0,dz);
  dz=Base(NI[0],NI[1])-Base(-24,-60);NAt(165827,NI[0],NI[1],dz);NM(165836,0,0,dz);NM(166061,NI[0]+24,NI[1]+60,dz);
  // Roof moved independently by the owner: bring its walls, slab and wall details with it.
  double dx=NL[0]+30,dy=NL[1]-6;dz=Base(NL[0],NL[1])-Base(-30,6);
  foreach(long id in new long[]{154895,154896,154897,154898}){var w=Get<Wall>(id);foreach(var a in w.GetAttachmentIds(AttachmentLocation.Top).ToList())w.RemoveAttachment(a);}
  foreach(long id in new long[]{154895,154896,154897,154898,154747,165264})NM(id,dx,dy,dz);NM(155386,0,0,dz);D.Regenerate();foreach(long id in new long[]{154895,154896,154897,154898})Get<Wall>(id).AddAttachment(E(155386),AttachmentLocation.Top);
  foreach(long id in new long[]{165657,165666,165712,165715,165720,165827,165836,166061})NMark(D.GetElement(E(id)),"Posicion alineada con movimiento del propietario");
 });}
 static void NShelters(){Tx("Caballerizas y cocina triangular observadas",()=>{
  NRemove("Caballerizas");NRemove("Cocina");Del(166045);Del(166060);
  double x=NS[0],y=NS[1],z=Base(x,y);var floor=Get<Floor>(154763);floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set((z+.03)*U);NMark(floor,"Caballerizas - solera en posicion del propietario");
  var roof=SiteRoof("Caballerizas - cubierta baja",x,y,8.0,5.4,z+2.35,.10,false,"Verde gallinero");NMark(roof,"Caballerizas - cubierta baja");var g=new List<GeometryObject>();
  g.Add(Box(x-3.5,y+1.80,z,7,.15,2.15,"Blanco"));
  for(int i=0;i<4;i++){double xx=x-3.5+i*1.75;g.Add(Box(xx,y-1.7,z,.12,3.5,2.12,"Blanco"));if(i>0){g.Add(Box(xx+.12,y-1.75,z,.20,.14,2.1,"Blanco"));g.Add(Box(xx+1.43,y-1.75,z,.32,.14,2.1,"Blanco"));g.Add(Box(xx+.32,y-1.75,z+1.95,1.11,.14,.17,"Blanco"));g.Add(Box(xx+.35,y-1.78,z+.02,.78,.09,1.87,"Verde gallinero"));g.Add(Box(xx+1.15,y-1.78,z+1.10,.28,.09,.68,"Verde gallinero"));}}
  g.Add(Box(x+3.4,y-1.7,z,.12,3.5,2.12,"Blanco"));for(double zz=.25;zz<1.1;zz+=.23)g.Add(Box(x-3.5,y-1.86,z+zz,1.7,.09,.10,"Madera"));foreach(double xx in new[]{x-3.5,x-1.75,x+3.5})g.Add(Rod(P(xx,y-2.1,z),P(xx,y-2.1,z+2.38),.065,"Tronco",8));Shape("1210 Caballerizas - divisiones y puertas verdes",g);
  x=NK[0];y=NK[1];z=Base(x,y);var f=SiteSlab("Cocina - solera",Rectangle(x,y,5.4,5.7),z+.04,"Concreto");NMark(f,"Cocina - solera");
  var r=SiteRoof("Cocina triangular - cubierta verde",x,y,6.1,6.1,z+2.12,.56,false,"Verde gallinero");NMark(r,"Cocina - cubierta triangular");
  var steel=new List<GeometryObject>();for(int iy=0;iy<3;iy++){double yy=y-2.55+iy*2.55;foreach(double s in new[]{-1d,1d}){
   double xx=x+s*2.35;steel.Add(Box(xx-.07,yy-.07,z,.14,.14,.20,"Concreto claro"));steel.Add(Rod(P(xx,yy,z+.1),P(xx,yy,z+2.54),.027,"Verde gallinero",4));steel.Add(Rod(P(xx+s*.14,yy,z+.1),P(xx+s*.14,yy,z+2.54),.023,"Verde gallinero",4));
   for(double h=.5;h<2.4;h+=.55)steel.Add(Rod(P(xx,yy,z+h),P(xx+s*.14,yy,z+h),.017,"Verde gallinero",4));
   XYZ e=P(x+s*3.0,yy,z+2.12),tip=P(x,yy,z+2.12+3.05*.56);steel.Add(Rod(e,tip,.035,"Verde gallinero",4));steel.Add(Rod(e-P(0,0,.2),tip-P(0,0,.2),.025,"Verde gallinero",4));for(int k=0;k<7;k++){var a=e+(tip-e)*(k/7d);var b=e+(tip-e)*((k+1)/7d)-P(0,0,.2);steel.Add(Rod(a,b,.019,"Verde gallinero",4));}
  }}
  for(double xx=x-2.85;xx<x+3;xx+=.76){double zz=z+2.12+(3.05-Math.Abs(xx-x))*.56+.14;steel.Add(Rod(P(xx,y-3.05,zz),P(xx,y+3.05,zz),.009,"Juntas cubierta",4));}
  Shape("1210 Cocina - estructura metalica verde",steel);
  var equip=new List<GeometryObject>();SiteMat("Acero cocina",156,165,163);SiteMat("Hueco fregadero",56,65,64);
  equip.Add(Box(x+1.45,y-.75,z,.6,2.8,.68,"Ladrillo portada"));equip.Add(Box(x+1.38,y-.82,z+.81,.78,2.95,.07,"Acero cocina"));equip.Add(Box(x+1.44,y-.66,z+.876,.6,.62,.006,"Hueco fregadero"));equip.Add(Box(x+1.44,y+.18,z+.876,.6,.62,.006,"Hueco fregadero"));equip.Add(Box(x+2.09,y-.82,z+.81,.06,2.95,.35,"Acero cocina"));
  foreach(double yy in new[]{y-.3,y+.55}){equip.Add(Rod(P(x+1.9,yy,z+.88),P(x+1.9,yy,z+1.20),.018,"Acero cocina",8));equip.Add(Rod(P(x+1.9,yy,z+1.20),P(x+1.70,yy,z+1.20),.018,"Acero cocina",8));}
  equip.Add(Box(x-.58,y+1.7,z+.30,1.16,.64,1.65,"Acero cocina"));for(int i=0;i<5;i++)equip.Add(Box(x-.50,y+1.65,z+.46+i*.29,1,.035,.035,"Carpinteria"));equip.Add(Box(x-.48,y+1.66,z+.45,.96,.025,1.35,"Vidrio"));
  equip.Add(Box(x-2.2,y+.55,z+.80,1.65,.65,.07,"Blanco"));foreach(double xx in new[]{x-2.1,x-.65})foreach(double yy in new[]{y+.65,y+1.1})equip.Add(Rod(P(xx,yy,z),P(xx,yy,z+.8),.025,"Acero cocina",6));equip.Add(Rod(P(x-.4,y-.45,z),P(x-.4,y-.45,z+.79),.24,"Tronco",24));equip.Add(Box(x-.92,y-.97,z+.79,1.04,1.04,.075,"Concreto"));
  Shape("1210 Cocina - fregadero horno y mesas",equip,BuiltInCategory.OST_Furniture);
 });}
 static void NLakes(){Tx("Lagos contiguos con bordes de ladrillo",()=>{
  foreach(long id in new long[]{165901,165902,165903,165913,165914,165915})Del(id);NRemove("Lago");
  SiteMat("Ladrillo lago",165,144,108);SiteMat("Junta lago",203,190,158);
  for(int j=0;j<3;j++){double z=NLvl(j);var ring=new Mesh();var joints=new Mesh();int count=160;
   for(int i=0;i<count;i++){double a=i*2*Math.PI/count,b=(i+1)*2*Math.PI/count;double[] p=NPoint(j,a),q=NPoint(j,b),o=NPoint(j,a,1.055),v=NPoint(j,b,1.055);
    ring.Quad(P(p[0],p[1],z+.13),P(q[0],q[1],z+.13),P(v[0],v[1],z+.13),P(o[0],o[1],z+.13),"Borde lago");
    ring.Quad(P(o[0],o[1],z-.85),P(v[0],v[1],z-.85),P(v[0],v[1],z+.13),P(o[0],o[1],z+.13),j==2?"Piedra oscura":"Ladrillo lago");
    if(j<2)for(int row=0;row<10;row++){double h=z+.08-row*.09;double[] jo=NPoint(j,a,1.056),jv=NPoint(j,b,1.056);joints.Quad(P(jo[0],jo[1],h),P(jv[0],jv[1],h),P(jv[0],jv[1],h+.006),P(jo[0],jo[1],h+.006),"Junta lago");if((i+row)%3==0){double c=a+.0016;var jq=NPoint(j,c,1.056);joints.Quad(P(jo[0],jo[1],h),P(jq[0],jq[1],h),P(jq[0],jq[1],h+.084),P(jo[0],jo[1],h+.084),"Junta lago");}}
   }
   var gg=new List<GeometryObject>();gg.AddRange(ring.Build());gg.AddRange(joints.Build());Shape("1210 Lago "+(j+1)+" - vaso y borde",gg);
   var pts=Enumerable.Range(0,count).Select(i=>{var p=NPoint(j,i*2*Math.PI/count);return P(p[0],p[1],z-.015);}).ToList();var solid=GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>{Loop(pts)},XYZ.BasisZ,.03*U,new SolidOptions(M("Agua sitio opaca"),ElementId.InvalidElementId));Shape("1210 Lago "+(j+1)+" - agua",new List<GeometryObject>{solid});
   if(j<2){var q=NP[j];var pipes=new List<GeometryObject>{Rod(P(q[0],q[1],z-.15),P(q[0],q[1],z+.73),.025,"Blanco",10),Rod(P(q[0],q[1],z+.73),P(q[0]+.65,q[1],z+.73),.024,"Blanco",10)};Shape("1210 Lago "+(j+1)+" - tuberia visible",pipes);}
  }
 });}
 static void NTerrain(){Tx("Terreno adaptado a ubicaciones corregidas",()=>{
  var old=new FilteredElementCollector(D).OfClass(typeof(Toposolid)).Cast<Toposolid>().Single();var type=old.GetTypeId();D.Delete(old.Id);
  var pts=new Dictionary<string,XYZ>();Action<double,double> add=(x,y)=>pts[x.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+","+y.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)]=P(x,y,NHeight(x,y));
  for(double x=-110;x<=110;x+=5)for(double y=-95;y<=105;y+=5)add(x,y);
  for(double x=-19;x<=21;x+=1)for(double y=5;y<=44;y+=1)add(x,y);
  foreach(var p in new[]{new[]{NC[0],NC[1],8d,17},new[]{NT[0],NT[1],9d,8},new[]{NS[0],NS[1],10d,7},new[]{NK[0],NK[1],8d,8},new[]{NL[0],NL[1],13d,11},new[]{NI[0],NI[1],14d,14}})for(double x=p[0]-p[2]/2;x<=p[0]+p[2]/2;x+=1)for(double y=p[1]-p[3]/2;y<=p[1]+p[3]/2;y+=1)add(x,y);
  foreach(var q in NP)for(double x=q[0]-q[2]*2;x<=q[0]+q[2]*2;x+=.65)for(double y=q[1]-q[3]*2;y<=q[1]+q[3]*2;y+=.65)add(x,y);
  for(int j=0;j<3;j++)for(int i=0;i<128;i++)foreach(double r in new[]{.97,1.02,1.40}){var p=NPoint(j,i*Math.PI/64,r);add(p[0],p[1]);}
  for(double x=-11;x<=11;x+=1)for(double y=-9;y<=7;y+=1)add(x,y);
  var topo=Toposolid.Create(D,pts.Values.ToList(),type,L.Id);NMark(topo,"Relieve para posiciones del propietario y recorrido posterior");
  RemoveShape("Borduras vegetales del recorrido principal");
  var removed=new List<long>();foreach(var plant in new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(e=>e.Category.Id.Value==(long)BuiltInCategory.OST_Planting).ToList()){
   var b=plant.get_BoundingBox(null);if(b==null)continue;var c=(b.Min+b.Max)*.1524;double x=c.X,y=c.Y;
   bool hit=NP.Any(q=>Math.Pow((x-q[0])/(q[2]+.55),2)+Math.Pow((y-q[1])/(q[3]+.55),2)<1)||new[]{new[]{NC[0],NC[1],4d,8.0},new[]{NT[0],NT[1],4d,3.3},new[]{NS[0],NS[1],4.6,3.2},new[]{NK[0],NK[1],3.5,3.5}}.Any(q=>Math.Abs(x-q[0])<q[2]&&Math.Abs(y-q[1])<q[3]);
   if(hit){removed.Add(plant.Id.Value);D.Delete(plant.Id);continue;}
   if(b.Max.X-b.Min.X<45*U&&plant.ApplicationDataId!="Borduras vegetales del recorrido principal"){double dz=NHeight(x,y)-b.Min.Z*.3048;if(Math.Abs(dz)>.07&&Math.Abs(dz)<5)ElementTransformUtils.MoveElement(D,plant.Id,P(0,0,dz));}
  }Notes.Add("Plantas en conflicto recolocadas mediante limpieza de "+removed.Count+" ejemplares esquematicos");
 });}
 static List<double[]> NRouteSample(double[][] line,double spacing){var p=new List<double[]>();for(int i=0;i<line.Length-1;i++){var a=line[i];var b=line[i+1];int count=(int)Math.Ceiling(Math.Sqrt(Math.Pow(b[0]-a[0],2)+Math.Pow(b[1]-a[1],2))/spacing);for(int j=0;j<count;j++){double t=(double)j/count;p.Add(new[]{a[0]+t*(b[0]-a[0]),a[1]+t*(b[1]-a[1]),a[2]+t*(b[2]-a[2])});}}p.Add(line.Last());return p;}
 static Floor NWalkFloor(string n,IList<XYZ> points){var type=new FilteredElementCollector(D).OfClass(typeof(FloorType)).Cast<FloorType>().FirstOrDefault(f=>f.Name=="Capilla nueva - cemento sendero");if(type==null){type=(FloorType)Get<FloorType>(1238).Duplicate("Capilla nueva - cemento sendero");var cs=type.GetCompoundStructure();for(int i=0;i<cs.LayerCount;i++){cs.SetMaterialId(i,M("Concreto"));if(i==0)cs.SetLayerWidth(i,.10*U);}type.SetCompoundStructure(cs);}var f=Floor.Create(D,new List<CurveLoop>{Loop(points.Select(p=>new XYZ(p.X,p.Y,0)))},type.Id,L.Id);D.Regenerate();var ed=f.GetSlabShapeEditor();ed.Enable();D.Regenerate();foreach(SlabShapeVertex v in ed.SlabShapeVertices){var p=points.OrderBy(p=>Math.Pow(p.X-v.Position.X,2)+Math.Pow(p.Y-v.Position.Y,2)).First();ed.ModifySubElement(v,p.Z);}NMark(f,n);return f;}
 static void NTuft(Mesh mesh,double x,double y,double ground){for(int k=0;k<12;k++){double a=k*Math.PI/6+.17*Math.Sin(x+y),dx=Math.Cos(a),dy=Math.Sin(a);var last=P(x,y,ground);for(int j=1;j<=3;j++){double t=j/3d,r=.34*t*t;var cur=P(x+dx*r,y+dy*r,ground+.47*Math.Sin(t*1.8));var side=P(-dy*.012,dx*.012,0);mesh.Quad(last-side,last+side,cur+side,cur-side,k%3==0?"Blanco":"Borde vegetal");last=cur;}}}
 static void NPaths(){Tx("Salida posterior escalones y sendero junto a lagos",()=>{
  NRemove("Recorrido");NRemove("Porche");Del(165918);RemoveShape("Bordillos y juntas de pavimentos");
  // Add the observed rear veranda and a real hosted door without reconstructing unseen rooms.
  foreach(var f in new FilteredElementCollector(D).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().Where(f=>(f.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").StartsWith("NUEVAS_1210 - Salida trasera")).ToList())D.Delete(f.Id);
  var door=D.Create.NewFamilyInstance(P(0,4.5,0),Get<FamilySymbol>(67177),Get<Wall>(154885),L,Autodesk.Revit.DB.Structure.StructuralType.NonStructural);NMark(door,"Salida trasera hacia lagos");
  var veranda=SiteSlab("Porche posterior",Rectangle(0,5.525,18,2.05),.055,"Terracota baldosa");NMark(veranda,"Porche posterior - piso");
  foreach(long id in new long[]{154879,154882,154885,154888})foreach(var a in Get<Wall>(id).GetAttachmentIds(AttachmentLocation.Top).ToList())Get<Wall>(id).RemoveAttachment(a);Del(163468);
  foreach(var r in new FilteredElementCollector(D).OfClass(typeof(RoofBase)).Where(r=>(r.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").StartsWith("NUEVAS_1210 - Porche - cubierta casa")).ToList())D.Delete(r.Id);
  var roof=SiteRoof("Casa principal con porche posterior",0,-.475,20,15.15,3.08,.23,true,"Techo verde");NMark(roof,"Porche - cubierta casa prolongada");foreach(long id in new long[]{154879,154882,154885,154888})Get<Wall>(id).AddAttachment(roof.Id,AttachmentLocation.Top);
  RemoveShape("Aleros canalones y bajantes");RemoveShape("Juntas de laminas limatesas y cumbrera");var detail=new List<GeometryObject>();
  foreach(double y in new[]{-8.05,7.1})detail.Add(Box(-10,y,3.05,20,.12,.16,"Baranda"));foreach(double x in new[]{-10d,9.94})detail.Add(Box(x,-8.05,3.05,.12,15.15,.16,"Baranda"));
  Func<double,double,double> rz=(x,y)=>3.08+.23*Math.Min(Math.Min(x+10,10-x),Math.Min(y+8.05,7.1-y))+.145;
  for(double x=-9.75;x<10;x+=.76)for(double y=-7.96;y<7.08;y+=.5){double yn=Math.Min(y+.5,7.08);detail.Add(Rod(P(x,y,rz(x,y)),P(x,yn,rz(x,yn)),.012,"Juntas cubierta",4));}
  foreach(double x in new[]{-9d,-6,-3,-.8,.8,3,6,9})detail.Add(Box(x-.065,6.43,.055,.13,.13,2.97,"Madera oscura"));detail.Add(Box(-9.1,6.42,2.89,18.2,.18,.2,"Madera oscura"));
  foreach(double side in new[]{-1d,1d}){double a=side<0?-9:.8,b=side<0?-.8:9;detail.Add(Box(a,6.44,.94,b-a,.1,.085,"Madera oscura"));detail.Add(Box(a,6.44,.22,b-a,.08,.07,"Madera oscura"));for(double x=a+.1;x<b;x+=.18)detail.Add(Box(x,6.45,.28,.04,.045,.65,"Madera oscura"));}
  for(double x=-8.99;x<9;x+=.48)for(double y=4.51;y<6.54;y+=.24)detail.Add(Box(x,y,.056,Math.Min(.473,9-x),Math.Min(.233,6.54-y),.012,"Terracota baldosa"));
  Shape("1210 Porche - estructura baranda baldosas y cubierta",detail);
  var ceiling=new Mesh();for(double y=4.55;y<6.9;y+=.14)ceiling.Quad(P(-9.3,y,2.97),P(9.3,y,2.97),P(9.3,y+.132,2.97),P(-9.3,y+.132,2.97),"Madera cielo");Shape("1210 Porche - cielo de madera",ceiling.Build());
  var steps=new List<GeometryObject>();for(int i=0;i<11;i++){double y=6.55+i*.38,top=.08+(i+1)*.15,bottom=Math.Min(NHeight(-.65,y),NHeight(.65,y))-.13;steps.Add(Box(-.65,y,bottom,1.30,.384,top-bottom,"Concreto"));}Shape("1210 Recorrido - once escalones posteriores aproximados",steps);
  var route=NRouteSample(RearRoute,1.15);var xy=route.Select(p=>new[]{p[0],p[1]}).ToList();var left=Offset(xy,.6);var right=Offset(xy,-.6);var edges=new List<GeometryObject>();var grass=new Mesh();
  for(int i=0;i<route.Count-1;i++){
   var a=route[i];var b=route[i+1];NWalkFloor("Recorrido posterior - tramo "+i,new[]{P(left[i][0],left[i][1],a[2]),P(right[i][0],right[i][1],a[2]),P(right[i+1][0],right[i+1][1],b[2]),P(left[i+1][0],left[i+1][1],b[2])});
   foreach(var side in new[]{left,right})edges.Add(Rod(P(side[i][0],side[i][1],a[2]-.015),P(side[i+1][0],side[i+1][1],b[2]-.015),.035,"Concreto claro",4));
  }
  foreach(double side in new[]{-1d,1d})foreach(var p in Offset(xy,side*.9))NTuft(grass,p[0],p[1],NHeight(p[0],p[1]));
  foreach(double side in new[]{-1d,1d})for(double y=6.7;y<10.8;y+=.40)NTuft(grass,side*.98,y,NHeight(side*.98,y));
  Shape("1210 Recorrido - bordes de cemento",edges);Shape("1210 Recorrido - borduras claras observadas",grass.Build(),BuiltInCategory.OST_Planting);
  var bench=new List<GeometryObject>();double bx=-10.4,by=29.1,bz=NHeight(bx,by);bench.Add(Box(bx-.7,by,bz+.43,1.4,.43,.055,"Blanco"));for(double xx=bx-.65;xx<=bx+.7;xx+=.32){bench.Add(Rod(P(xx,by+.39,bz+.48),P(xx,by+.39,bz+.99),.014,"Blanco",8));}foreach(double xx in new[]{bx-.62,bx+.62})foreach(double yy in new[]{by+.04,by+.38})bench.Add(Rod(P(xx,yy,bz),P(xx,yy,bz+.45),.018,"Blanco",8));Shape("1210 Recorrido - banco junto al lago",bench,BuiltInCategory.OST_Furniture);
 });}
 static void NViews(){Tx("Vistas nuevas tomas y correcciones de ubicacion",()=>{
  Camera("CAPILLA - 12 Recorrido trasero y dos lagos",C(42,-30,48),C(1,21,2.5),C(-18,-10,-3),C(39,46,24));
  Camera("CAPILLA - 13 Caballerizas y cocina",C(14,15,17),C(-3,36,6),C(-16,29,1),C(6,43,15));
  Camera("CAPILLA - 14 Gallinero y tanques reubicados",C(70,15,45),C(33,45,7),C(19,9,-1),C(58,72,25));
  Camera("CAPILLA - 15 Salida trasera hacia lagos",C(-17,28,14),C(0,7,1.3),C(-13,-9,-1),C(18,33,16));
  Camera("CAPILLA - 16 Conjunto nuevas tomas",C(54,-128,93),C(4,3,2),C(-66,-73,-15),C(67,77,33));
  var template=new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().First(v=>v.Name=="CAPILLA - Presentacion con sombras");
  foreach(var v in new FilteredElementCollector(D).OfClass(typeof(View3D)).Cast<View3D>().Where(v=>!v.IsTemplate&&Enumerable.Range(12,5).Any(i=>v.Name.StartsWith("CAPILLA - "+i)))){
   v.ApplyViewTemplateParameters(template);var box=v.GetSectionBox();var hide=new List<ElementId>();foreach(var e in new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(e=>e.Category.Id.Value==(long)BuiltInCategory.OST_Planting)){var b=e.get_BoundingBox(null);var c=(b.Min+b.Max)/2;if(c.X<box.Min.X||c.X>box.Max.X||c.Y<box.Min.Y||c.Y>box.Max.Y||b.Max.Z>box.Max.Z)if(e.CanBeHidden(v))hide.Add(e.Id);}if(hide.Count>0)v.HideElements(hide);
   foreach(Category cat in D.Settings.Categories)foreach(Category sub in cat.SubCategories)if(sub.Name.IndexOf("curva",StringComparison.OrdinalIgnoreCase)>=0||sub.Name.IndexOf("contour",StringComparison.OrdinalIgnoreCase)>=0)try{v.SetCategoryHidden(sub.Id,true);}catch{}
  }
 });}
 static void NExtras(){Tx("Huellas de cemento y caseta al final del recorrido",()=>{
  Del(164479);NRemove("Huellas");NRemove("Servicio");
  var line=Sample(DriveRoute,1.5);var joints=new List<GeometryObject>();
  foreach(double s in new[]{-1d,1d}){var center=Offset(line,s*.70);var a=Offset(center,.45);var b=Offset(center,-.45);for(int i=0;i<line.Count-1;i++)NWalkFloor("Huellas de cemento - "+s+" - "+i,new[]{P(a[i][0],a[i][1],NHeight(a[i][0],a[i][1])+.14),P(b[i][0],b[i][1],NHeight(b[i][0],b[i][1])+.14),P(b[i+1][0],b[i+1][1],NHeight(b[i+1][0],b[i+1][1])+.14),P(a[i+1][0],a[i+1][1],NHeight(a[i+1][0],a[i+1][1])+.14)});}
  foreach(double s in new[]{-1d,1d}){var p=Offset(line,s*1.40);for(int i=0;i<p.Count-1;i++)joints.Add(Rod(P(p[i][0],p[i][1],NHeight(p[i][0],p[i][1])+.07),P(p[i+1][0],p[i+1][1],NHeight(p[i+1][0],p[i+1][1])+.07),.045,"Concreto",4));}Shape("1210 Huellas - cunetas laterales",joints);
  double x=-14.2,y=40.3,z=5.17;var slab=SiteSlab("Servicio - piso",Rectangle(x,y,1.9,1.9),z+.03,"Concreto");NMark(slab,"Servicio - piso de caseta observada");var roof=SiteRoof("Caseta de servicio",x,y,2.1,2.0,z+2.25,.06,false,"Metal gris");NMark(roof,"Servicio - cubierta");
  var g=new List<GeometryObject>{Box(x-.75,y-.7,z,.12,1.5,2.15,"Blanco"),Box(x+.63,y-.7,z,.12,1.5,2.15,"Blanco"),Box(x-.75,y+.68,z,1.5,.12,2.15,"Blanco"),Box(x-.75,y-.7,z,.35,.12,2.15,"Blanco"),Box(x+.37,y-.7,z,.38,.12,2.15,"Blanco"),Box(x-.4,y-.7,z+2.0,.77,.12,.15,"Blanco"),Box(x-.38,y-.73,z+.02,.73,.08,1.98,"Madera cielo")};Shape("1210 Servicio - caseta blanca puerta de madera",g);
 });}
 static void NGroups(){Tx("Agrupar componentes para futuros movimientos",()=>{
  Action<string,IEnumerable<ElementId>> group=(n,ids)=>{var list=ids.Where(id=>D.GetElement(id)!=null&&D.GetElement(id).GroupId==ElementId.InvalidElementId).Distinct().ToList();if(list.Count>1){var g=D.Create.NewGroup(list);g.GroupType.Name="CAPILLA - "+n;NMark(g,"Conjunto movible "+n);}};
  group("Gallinero",new[]{E(165657),E(165666),E(165712)});group("Tanques",new[]{E(165715),E(165720)});group("Kiosco inferior",new[]{E(165827),E(165836),E(166061)});
  for(int j=1;j<=3;j++)group("Lago "+j,new FilteredElementCollector(D).OfClass(typeof(DirectShape)).Cast<DirectShape>().Where(e=>e.ApplicationDataId.StartsWith("1210 Lago "+j+" - ")).Select(e=>e.Id));
  foreach(string n in new[]{"Caballerizas","Cocina"})group(n,new FilteredElementCollector(D).WhereElementIsNotElementType().Where(e=>e is Floor||e is RoofBase||e is DirectShape).Where(e=>(e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString()??"").StartsWith("NUEVAS_1210 - "+n)||(e as DirectShape)?.ApplicationDataId.StartsWith("1210 "+n+" - ")==true).Select(e=>e.Id));
 });}
