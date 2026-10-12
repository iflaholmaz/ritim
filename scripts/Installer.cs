using System;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Reflection;

class Setup : Form {
 static string InstallDir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Programs","Ritim");
 static string DesktopLink=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"Ritim.lnk");
 static string MenuLink=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),"Programs","Ritim.lnk");
 const string Key="Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Ritim";
 Label status; ProgressBar progress; Button install; bool busy=false;
 [STAThread] static void Main(string[] args) {
  if(Array.IndexOf(args,"--remove-dir")>=0){for(int i=0;i<30;i++){try{if(Directory.Exists(InstallDir))Directory.Delete(InstallDir,true);break;}catch{System.Threading.Thread.Sleep(500);}}return;}
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  if(Array.IndexOf(args,"--uninstall")>=0){Uninstall();return;}
  if(Array.IndexOf(args,"--silent")>=0){try{Install(null);File.WriteAllText(Path.Combine(Path.GetTempPath(),"ritim-install-result.txt"),"PASS");}catch(Exception e){File.WriteAllText(Path.Combine(Path.GetTempPath(),"ritim-install-result.txt"),"FAIL "+e);Environment.ExitCode=1;}return;}
  Application.Run(new Setup());
 }
 Setup(){Text="Ritim Kurulumu";ClientSize=new Size(520,340);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;StartPosition=FormStartPosition.CenterScreen;BackColor=Color.FromArgb(247,248,242);Font=new Font("Segoe UI",10);try{Icon=new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("app.ico"));}catch{}
  var title=new Label{Text="Ritim",Font=new Font("Segoe UI",30,FontStyle.Bold),ForeColor=Color.FromArgb(36,76,60),Location=new Point(34,26),AutoSize=true};Controls.Add(title);
  var desc=new Label{Text="Günlük yaşamın için küçük bir yardımcı.\nSu, kilo, öğünler ve alışkanlıkların tek yerde.",Location=new Point(38,99),Size=new Size(440,58)};Controls.Add(desc);
  status=new Label{Text="Kurulum masaüstüne bir Ritim kısayolu ekler.\nMevcut takip kayıtların korunur.",Location=new Point(38,178),Size=new Size(440,55),ForeColor=Color.FromArgb(110,124,111)};Controls.Add(status);
  progress=new ProgressBar{Location=new Point(38,237),Size=new Size(440,8),Visible=false};Controls.Add(progress);
  install=new Button{Text="Kur ve aç",Location=new Point(335,270),Size=new Size(143,42),BackColor=Color.FromArgb(36,76,60),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};install.FlatAppearance.BorderSize=0;install.Click+=async(s,e)=>{if(busy)return;busy=true;install.Enabled=false;progress.Visible=true;status.Text="Ritim kuruluyor…";try{await Task.Run(()=>Install(n=>BeginInvoke(new Action(()=>{progress.Value=n;status.Text="Ritim kuruluyor… %"+n;}))));status.Text="Kurulum tamamlandı. Ritim açılıyor.";Process.Start(Path.Combine(InstallDir,"Ritim.exe"));busy=false;Close();}catch(Exception ex){busy=false;install.Enabled=true;status.Text="Kurulum tamamlanamadı.";MessageBox.Show(ex.Message,"Ritim",MessageBoxButtons.OK,MessageBoxIcon.Error);}};Controls.Add(install);FormClosing+=(s,e)=>{if(busy)e.Cancel=true;};
 }
 static void Install(Action<int> report){
  Directory.CreateDirectory(Path.GetDirectoryName(InstallDir));string stage=InstallDir+".kurulum-"+Guid.NewGuid().ToString("N"),backup=InstallDir+".onceki-"+Guid.NewGuid().ToString("N");
  foreach(var p in Process.GetProcessesByName("Ritim")){try{string exe=p.MainModule.FileName;if(String.Equals(exe,Path.Combine(InstallDir,"Ritim.exe"),StringComparison.OrdinalIgnoreCase)){Process.Start(new ProcessStartInfo(exe,"--quit-for-install"){CreateNoWindow=true});if(!p.WaitForExit(5000))throw new Exception("Önce sistem tepsisinden Ritim’i tamamen kapat, ardından kurulumu tekrar başlat.");}}catch(System.ComponentModel.Win32Exception){}}
  bool moved=false;try{Directory.CreateDirectory(stage);using(var source=Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.zip")){if(source==null)throw new Exception("Kurulum paketi bulunamadı.");using(var zip=new ZipArchive(source,ZipArchiveMode.Read)){int i=0;foreach(var entry in zip.Entries){string relative=entry.FullName.Replace('/',Path.DirectorySeparatorChar);if(!relative.StartsWith("Ritim"+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))continue;relative=relative.Substring(6);string dest=Path.GetFullPath(Path.Combine(stage,relative));if(!dest.StartsWith(Path.GetFullPath(stage)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)&&dest!=stage)throw new Exception("Geçersiz paket yolu.");if(String.IsNullOrEmpty(entry.Name)){Directory.CreateDirectory(dest);}else{Directory.CreateDirectory(Path.GetDirectoryName(dest));entry.ExtractToFile(dest,true);}if(report!=null)report(Math.Min(90,++i*90/zip.Entries.Count));}}}
   if(!File.Exists(Path.Combine(stage,"Ritim.exe")))throw new Exception("Uygulama dosyası eksik.");
   using(var u=Assembly.GetExecutingAssembly().GetManifestResourceStream("uninstall.exe")){if(u==null)throw new Exception("Kaldırma aracı bulunamadı.");using(var f=File.Create(Path.Combine(stage,"Ritim-Kaldir.exe")))u.CopyTo(f);}
   if(Directory.Exists(InstallDir)){Directory.Move(InstallDir,backup);moved=true;}Directory.Move(stage,InstallDir);
   Shortcut(DesktopLink);Shortcut(MenuLink);
   using(var key=Registry.CurrentUser.CreateSubKey(Key)){key.SetValue("DisplayName","Ritim");key.SetValue("DisplayVersion","1.3.0");key.SetValue("Publisher","Ritim");key.SetValue("InstallLocation",InstallDir);key.SetValue("DisplayIcon",Path.Combine(InstallDir,"Ritim-v2.ico"));key.SetValue("UninstallString","\""+Path.Combine(InstallDir,"Ritim-Kaldir.exe")+"\" --uninstall");key.SetValue("NoModify",1);key.SetValue("NoRepair",1);}
   if(moved)try{Directory.Delete(backup,true);}catch{}if(report!=null)report(100);
  }catch{if(Directory.Exists(stage))try{Directory.Delete(stage,true);}catch{}if(moved&&Directory.Exists(backup)){if(Directory.Exists(InstallDir))try{Directory.Delete(InstallDir,true);}catch{}if(!Directory.Exists(InstallDir))Directory.Move(backup,InstallDir);}throw;}
 }
 static void Shortcut(string link){Type t=Type.GetTypeFromProgID("WScript.Shell");object shell=Activator.CreateInstance(t);object sc=t.InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{link});Type c=sc.GetType();c.InvokeMember("TargetPath",BindingFlags.SetProperty,null,sc,new object[]{Path.Combine(InstallDir,"Ritim.exe")});c.InvokeMember("WorkingDirectory",BindingFlags.SetProperty,null,sc,new object[]{InstallDir});c.InvokeMember("IconLocation",BindingFlags.SetProperty,null,sc,new object[]{Path.Combine(InstallDir,"Ritim-v2.ico")});c.InvokeMember("Save",BindingFlags.InvokeMethod,null,sc,null);}
 static void Uninstall(){if(MessageBox.Show("Ritim kaldırılsın mı?\nTakip kayıtların bilgisayarında korunacak.","Ritim",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;try{foreach(var p in Process.GetProcessesByName("Ritim")){try{if(p.MainModule.FileName==Path.Combine(InstallDir,"Ritim.exe")){Process.Start(new ProcessStartInfo(p.MainModule.FileName,"--quit-for-install"){CreateNoWindow=true});if(!p.WaitForExit(5000))throw new Exception("Önce Ritim’i sistem tepsisinden tamamen kapat.");}}catch(System.ComponentModel.Win32Exception){}}if(File.Exists(DesktopLink))File.Delete(DesktopLink);if(File.Exists(MenuLink))File.Delete(MenuLink);Registry.CurrentUser.DeleteSubKeyTree(Key,false);using(var run=Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",true)){if(run!=null){foreach(var name in run.GetValueNames()){string val=Convert.ToString(run.GetValue(name));if(val.IndexOf(Path.Combine(InstallDir,"Ritim.exe"),StringComparison.OrdinalIgnoreCase)>=0)run.DeleteValue(name,false);}}}string helper=Path.Combine(Path.GetTempPath(),"Ritim-Kaldir-"+Guid.NewGuid().ToString("N")+".exe");File.Copy(Application.ExecutablePath,helper);Process.Start(new ProcessStartInfo(helper,"--remove-dir"){CreateNoWindow=true});}catch(Exception e){MessageBox.Show(e.Message,"Ritim");}}
}




