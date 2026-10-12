using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
class IconPatch {
 delegate bool EnumName(IntPtr module,IntPtr type,IntPtr name,IntPtr extra);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr LoadLibraryEx(string name,IntPtr file,uint flags);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern bool EnumResourceNames(IntPtr module,IntPtr type,EnumName callback,IntPtr extra);
 [DllImport("kernel32.dll")]static extern bool FreeLibrary(IntPtr module);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr BeginUpdateResource(string name,bool deleteExisting);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool UpdateResource(IntPtr update,IntPtr type,IntPtr name,ushort language,byte[] bytes,uint size);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool EndUpdateResource(IntPtr update,bool discard);
 static int Main(string[] args){if(args.Length!=2)return 2;try{byte[] ico=File.ReadAllBytes(args[1]);if(BitConverter.ToUInt16(ico,2)!=1)throw new Exception("Expected ICO");int count=BitConverter.ToUInt16(ico,4);var names=new List<Tuple<int,IntPtr>>();IntPtr lib=LoadLibraryEx(args[0],IntPtr.Zero,2);foreach(int type in new[]{3,14}){int captured=type;EnumResourceNames(lib,(IntPtr)type,(m,t,n,x)=>{if(n.ToInt64()<=65535)names.Add(Tuple.Create(captured,n));return true;},IntPtr.Zero);}FreeLibrary(lib);IntPtr update=BeginUpdateResource(args[0],false);if(update==IntPtr.Zero)throw new System.ComponentModel.Win32Exception();bool committed=false;try{foreach(var n in names)UpdateResource(update,(IntPtr)n.Item1,n.Item2,1033,null,0);using(var stream=new MemoryStream())using(var writer=new BinaryWriter(stream)){writer.Write((ushort)0);writer.Write((ushort)1);writer.Write((ushort)count);for(int i=0;i<count;i++){int offset=6+i*16;uint size=BitConverter.ToUInt32(ico,offset+8),position=BitConverter.ToUInt32(ico,offset+12);byte[] blob=new byte[size];Array.Copy(ico,(int)position,blob,0,(int)size);if(!UpdateResource(update,(IntPtr)3,(IntPtr)(i+1),1033,blob,size))throw new System.ComponentModel.Win32Exception();writer.Write(ico,offset,12);writer.Write((ushort)(i+1));}byte[] group=stream.ToArray();if(!UpdateResource(update,(IntPtr)14,(IntPtr)1,1033,group,(uint)group.Length))throw new System.ComponentModel.Win32Exception();}if(!EndUpdateResource(update,false))throw new System.ComponentModel.Win32Exception();committed=true;}finally{if(!committed)EndUpdateResource(update,true);}Console.WriteLine("Ritim executable icon updated");return 0;}catch(Exception e){Console.Error.WriteLine(e.Message);return 1;}}
}

