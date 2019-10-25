using LiteDatabase.AdvancedCache;
using LiteDatabase.CustomedCryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiteDatabase
{
    public enum DatabaseMode
    {
        OnDemand, Cache, SemiCache
    }
    //Local Two-Key DB "L2KDB"
    public class Database
    {
        DatabaseMode LoadMode;
        public readonly string Flavor = "Original";
        public readonly static Version DatabaseVersion = new Version(1, 0, 3, 0);
        public CryptographyCredential cryptographyCredential = new CryptographyCredential();
        public DirectoryInfo HomeDirectory = new DirectoryInfo("./Databases/");
        public string givenHome = "";
        public string GeneratedID = "";
        public CustomedAES aes = new CustomedAES();
        Queue<DatabaseOperation> OperationQueue = new Queue<DatabaseOperation>();
        bool willDo = false;
        bool DoCycleQueue = true;
        public void ForceProcessQueue()
        {
            willDo = true;
        }
        public void ForceProcessPueueAndEndCycle()
        {
            DoCycleQueue = false;
            SingleCycle();
        }
        public void SingleCycle()
        {

            List<string> QueuedID1s = new List<string>();
            Queue<DatabaseOperation> CopiedOperations = new Queue<DatabaseOperation>(OperationQueue.ToArray());
            OperationQueue.Clear();// Clear the queue to receive more operation applications without data lose;
            foreach (var item in CopiedOperations)
            {
                string id1 = item.id1;
                string id2 = item.id2;
                if (!QueuedID1s.Contains(id1)) ;
                QueuedID1s.Add(id1);
                //if (item.GetType() == typeof(DBOperationSave))
                //{
                //    //if*
                //}
                //else if (item.GetType() == typeof(DBOperationRemoveID2))
                //{
                //    //data[item.id1].Remove(item.id2);
                //}
            }
            {
                foreach (var item in QueuedID1s)
                {
                    //Regenerate an ID-1 file.
                    Dictionary<string, object> OriginData = data[item];
                    FileInfo fi = new FileInfo(Path.Combine(FormDirectory.FullName, item + ".lite-db"));
                    File.WriteAllText(fi.FullName, "");//Clear the file.
                    File.WriteAllText(fi.FullName, $"#Database.Ver={DatabaseVersion.Build}\r\n#Form={CurrentForm}{Environment.NewLine}#Flavor={Flavor}{Environment.NewLine}");
                    String toWrite = "";
                    StringBuilder builder = new StringBuilder(toWrite);
                    var textWriter = new StringWriter(builder);
                    foreach (var SingleData in OriginData)
                    {
                        textWriter.WriteLine($"#DATA:" + SingleData.Key);
                        if (cryptographyCredential.Key == "")
                        {

                            textWriter.WriteLine((string)SingleData.Value);

                        }
                        else
                        {
                            textWriter.WriteLine(aes.Encrypt((string)SingleData.Value));
                        }
                        textWriter.WriteLine("DATA#");
                    }
                    File.AppendAllText(fi.FullName, builder.ToString());
                    try
                    {

                        textWriter.Close();

                    }
                    catch (Exception)
                    {
                    }
                    try
                    {

                        textWriter.Dispose();

                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        public async void CycleQueueProcessor()
        {
            DateTime mark = DateTime.Now;
            string OperatingForm = CurrentForm;
            while (OperatingForm == CurrentForm&&DoCycleQueue==true)
            {
                if (DateTime.Now - mark >= new TimeSpan(1000))
                {
                    willDo = true;

                }
                else
                {
                    if (OperationQueue.Count > 1000)
                    {
                        willDo = true;
                    }
                }
                if (willDo == true)
                {
                    mark = DateTime.Now;
                    willDo = false;
                    SingleCycle();
                }
            }
        }
        public Database(String Home = "./Databases/", DatabaseMode loadMode = DatabaseMode.OnDemand, CryptographyCredential cryptographyCredential = null)
        {
            givenHome = Home;
            if (cryptographyCredential == null)
            {
                this.cryptographyCredential = new CryptographyCredential();
            }
            else
            {
                this.cryptographyCredential = cryptographyCredential;
                aes.Key = cryptographyCredential.Key;
                aes.IV = cryptographyCredential.IV;
            }
            HomeDirectory = new DirectoryInfo(Home);
            LoadMode = loadMode;
            switch (LoadMode)
            {
                case DatabaseMode.OnDemand:
                    {
                        //Do nothing.
                        //Things will only be loaded when they are needed.
                    }
                    break;
                case DatabaseMode.Cache:
                    break;
                case DatabaseMode.SemiCache:
                    break;
                default:
                    break;
            }
        }
        public List<String> GetForms()
        {
            List<string> vs = new List<string>();
            foreach (var item in HomeDirectory.GetDirectories())
            {
                vs.Add(item.Name);
            }
            return vs;
        }
        public void ClearForm(string formName)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(HomeDirectory.FullName, formName));
            foreach (var item in directoryInfo.EnumerateFiles())
            {
                item.Delete();
            }
        }
        public void DeleteForm(string formName)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(HomeDirectory.FullName, formName));
            directoryInfo.Delete(true);
        }
        public void RemoveID1(string ID1)
        {
            FileInfo directoryInfo = new FileInfo(Path.Combine(HomeDirectory.FullName, CurrentForm, ID1));
            directoryInfo.Delete();
        }
        public void ClearCurrentForm()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(HomeDirectory.FullName, CurrentForm));
            foreach (var item in directoryInfo.EnumerateFiles())
            {
                item.Delete();
            }
        }
        public List<string> GetID1()
        {
            List<string> vs = new List<string>();

            switch (LoadMode)
            {
                case DatabaseMode.OnDemand:

                    DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(HomeDirectory.FullName, CurrentForm));
                    foreach (var item in directoryInfo.EnumerateFiles())
                    {
                        var id1 = item.Name.Substring(0, item.Name.Length - ".lite-db".Length);
                        vs.Add(id1);
                    }
                    break;
                case DatabaseMode.Cache:
                    return data.Keys.ToList();
                case DatabaseMode.SemiCache:
                    break;
                default:
                    break;
            }
            return vs;
        }
        public void ClearID1(string id1)
        {
            File.WriteAllText(Path.Combine(HomeDirectory.FullName, CurrentForm, $"{id1}.lite-db"), $"#Database.Version:{DatabaseVersion.Build}\r\n#Form:{CurrentForm}\r\n#Flavor={Flavor}");
        }
        string CurrentForm = "";
        public string GetCurrentFormName() => CurrentForm;
        DirectoryInfo FormDirectory = new DirectoryInfo("./");
        public void OpenForm(string name = "DEFAULT")
        {
            FormDirectory = new DirectoryInfo(Path.Combine(HomeDirectory.FullName, name));
            data = new Dictionary<string, Dictionary<string, object>>();
            if (!FormDirectory.Exists)
                FormDirectory.Create();
            CurrentForm = name;
            switch (LoadMode)
            {
                case DatabaseMode.OnDemand:
                    break;
                case DatabaseMode.Cache:
                    {

                        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(HomeDirectory.FullName, CurrentForm));
                        foreach (var item in directoryInfo.EnumerateFiles())
                        {
                            var id1 = item.Name.Substring(0, item.Name.Length - ".lite-db".Length);
                            var d = LoadFile(id1);
                            data.Add(id1, d);
                        }
                        Task.Run(CycleQueueProcessor);
                    }
                    break;
                case DatabaseMode.SemiCache:
                    break;
                default:
                    break;
            }
        }
        Dictionary<string, object> LoadFile(string name)
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            FileInfo fi = new FileInfo(Path.Combine(FormDirectory.FullName, name + ".lite-db"));
            string Content = "";
            string id2 = "";
            var tR = fi.OpenText();
            bool StopReading = false;
            bool isCombine = false;
            bool isFSL = false;
            while (StopReading == false)
            {
                var tmp = tR.ReadLine();
                if (tmp == null)
                {
                    //Console.WriteLine("Cannot find:" + id2);
                    break;
                }
                if (isCombine == false)
                {
                    if (tmp.StartsWith("#DATA:"))
                    {
                        isCombine = true;
                        id2 = tmp.Substring("#DATA:".Length);
                    }
                }
                else
                {
                    if (tmp != "DATA#")
                    {
                        if (Content == "")
                        {
                            Content = tmp;
                        }
                        else
                        {
                            Content += "\r\n" + tmp;
                        }
                    }
                    else
                    {
                        isFSL = true;
                        isCombine = false;
                        if (cryptographyCredential.Key != "")
                            d.Add(id2, aes.Decrypt(Content));
                        else d.Add(id2, Content);
                        Content = "";
                        id2 = "";
                    }
                }
            }
            try
            {

                tR.Close();

            }
            catch (Exception)
            {
            }
            try
            {
                tR.Dispose();
            }
            catch (Exception)
            {
            }

            return d;
        }

        Dictionary<string, Dictionary<string, object>> data = new Dictionary<string, Dictionary<string, object>>();
        public List<string> GetID2(string id1)
        {
            switch (LoadMode)
            {
                case DatabaseMode.OnDemand:
                    {

                        List<string> ID2s = new List<string>();
                        FileInfo fi = new FileInfo(Path.Combine(FormDirectory.FullName, id1 + ".lite-db"));
                        var tR = fi.OpenText();

                        string tmp = "";
                        while ((tmp = tR.ReadLine()) != null)
                        {
                            if ((tmp).StartsWith("#DATA:"))
                            {
                                ID2s.Add(tmp.Substring("#DATA:".Length));
                            }
                        }
                        tR.Dispose();
                        return ID2s;
                    }
                case DatabaseMode.Cache:
                    {
                        return data[id1].Keys.ToList();
                    }
                case DatabaseMode.SemiCache:
                    break;
                default:
                    break;
            }
            return null;
        }
        public int GetCount(string id1)
        {
            switch (LoadMode)
            {
                case DatabaseMode.OnDemand:
                    {
                        int count = 0;
                        FileInfo fi = new FileInfo(Path.Combine(FormDirectory.FullName, id1 + ".lite-db"));
                        var tR = fi.OpenText();

                        string tmp;
                        while ((tmp = tR.ReadLine()) != null)
                        {
                            if ((tmp).StartsWith("#DATA:"))
                            {
                                count++;
                            }
                        }
                        tR.Dispose();
                        return count;
                    }
                case DatabaseMode.Cache:
                    {
                        return data[id1].Keys.Count;
                    }
                case DatabaseMode.SemiCache:
                    break;
                default:
                    break;
            }
            return -1;
        }
        public object Query(string id1, string id2)
        {
            switch (LoadMode)
            {
                case DatabaseMode.OnDemand:
                    {
                        string Content = "";
                        FileInfo fi = new FileInfo(Path.Combine(FormDirectory.FullName, id1 + ".lite-db"));
                        var tR = fi.OpenText();
                        bool StopReading = false;
                        bool isCombine = false;
                        bool isFSL = false;
                        while (StopReading == false)
                        {
                            var tmp = tR.ReadLine();
                            if (tmp == null)
                            {
                                //Console.WriteLine("Cannot find:" + id2);
                                tR.Dispose();
                                throw new Exception("Cannot find:" + id1 + "," + id2);
                            }
                            if (isCombine == false)
                            {
                                if (tmp == "#DATA:" + id2)
                                {
                                    isCombine = true;
                                }
                            }
                            else
                            {
                                if (tmp != "DATA#")
                                {
                                    if (Content == "")
                                    {
                                        Content = tmp;
                                    }
                                    else
                                    {


                                        Content += "\r\n" + tmp;

                                    }
                                    //if (isFSL != true)
                                    //{
                                    //    Content += tmp;
                                    //}
                                    //else
                                    //{}
                                }
                                else
                                {
                                    isCombine = false;
                                    StopReading = true;
                                    tR.Dispose();
                                    if (cryptographyCredential.Key != "")
                                        return aes.Decrypt(Content);
                                    else return Content;
                                }
                            }
                        }
                        return "";
                    }
                case DatabaseMode.Cache:
                    return data[id1][id2];
                //break;
                case DatabaseMode.SemiCache:
                    break;
                default:
                    break;
            }
            return null;
        }
        ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        public void Remove(string id1, string id2)
        {
            if (LoadMode == DatabaseMode.OnDemand)
            {

                FileInfo fi = new FileInfo(Path.Combine(FormDirectory.FullName, id1 + ".lite-db"));
                if (!fi.Exists) fi.Create().Dispose();
                var tR = File.ReadAllLines(fi.FullName).ToList();
                int length = tR.Count;
                bool logMode = false;
                int count = 1;
                int index = -1;
                for (int i = 0; i < tR.Count; i++)
                {
                    if (logMode == true)
                    {
                        count++;
                        if (tR[i] == "DATA#")
                        {
                            logMode = false;
                            break;
                        }
                        else
                        {
                        }
                    }
                    else
                    {

                        if (tR[i] == "#DATA:" + id2)
                        {
                            index = i;
                            logMode = true;
                        }
                    }
                }
                tR.RemoveRange(index, count);
                //Locker.EnterWriteLock();
                File.WriteAllLines(fi.FullName, tR);
                //Locker.ExitWriteLock();

            }
            else
            if (LoadMode == DatabaseMode.Cache)
            {
                data[id1].Remove(id2);
                OperationQueue.Enqueue(new DatabaseOperation() { id1 = id1, id2 = id2 });
            }
            else
            {
                data[id1].Remove(id2);

            }
        }
        public bool Save(string id1, string id2, Object content)
        {
            switch (LoadMode)
            {
                case DatabaseMode.OnDemand:
                    {
                        FileInfo fi = new FileInfo(Path.Combine(FormDirectory.FullName, id1 + ".lite-db"));
                        if (!fi.Exists) fi.Create().Dispose();
                        var tR = File.ReadAllLines(fi.FullName).ToList();
                        int length = tR.Count;
                        bool logMode = false;
                        int count = 1;
                        int index = -1;
                        for (int i = 0; i < tR.Count; i++)
                        {
                            if (logMode == true)
                            {
                                count++;
                                if (tR[i] == "DATA#")
                                {
                                    logMode = false;
                                    break;
                                }
                                else
                                {
                                }
                            }
                            else
                            {

                                if (tR[i] == "#DATA:" + id2)
                                {
                                    index = i;
                                    logMode = true;
                                }
                            }
                        }
                        if (index == -1)
                        {
                            if (cryptographyCredential.Key == "")
                            {

                                var cont = File.ReadAllText(fi.FullName);
                                if (cont == "")
                                    File.WriteAllText(fi.FullName, $"#Database.Ver={DatabaseVersion.Build}\r\n#Form={CurrentForm}{Environment.NewLine}#Flavor={Flavor}\r\n#DATA:{id2}\r\n{((string)content)}\r\nDATA#\r\n");
                                else
                                {
                                    File.AppendAllText(fi.FullName, $"#DATA:{id2}{Environment.NewLine}{((string)content)}\r\nDATA#\r\n");
                                }
                            }
                            else
                            {
                                var cont = File.ReadAllText(fi.FullName);
                                if (cont == "")
                                    File.WriteAllText(fi.FullName, $"#Database.Ver={DatabaseVersion.Build}\r\n#Form={CurrentForm}\r\n#Flavor={Flavor}\r\n#DATA:{id2}\r\n{aes.Encrypt((string)content)}\r\nDATA#\r\n");
                                else
                                {
                                    File.AppendAllText(fi.FullName, $"#DATA:{id2}\r\n{aes.Encrypt((string)content)}\r\nDATA#\r\n");
                                }
                            }
                        }
                        else
                        {
                            if (cryptographyCredential.Key == "")
                            {
                                tR.RemoveRange(index, count);
                                tR.Insert(index, $"#DATA:{id2}\r\n{((string)content)}\r\nDATA#");
                                //Locker.EnterWriteLock();
                                File.WriteAllLines(fi.FullName, tR);
                                //Locker.ExitWriteLock();
                            }
                            else
                            {

                                tR.RemoveRange(index, count);
                                tR.Insert(index, $"#DATA:{id2}\r\n{aes.Encrypt((string)content)}\r\nDATA#");
                                File.WriteAllLines(fi.FullName, tR);

                            }
                        }
                    }
                    break;
                case DatabaseMode.Cache:

                    {
                        if (data.ContainsKey(id1))
                        {
                            if (data[id1].ContainsKey(id2))
                            {
                                data[id1][id2] = content;
                            }
                            else
                            {
                                data[id1].Add(id2, content);
                            }
                        }
                        else
                        {
                            Dictionary<string, object> tmp = new Dictionary<string, object>();
                            tmp.Add(id2, content);
                            data.Add(id1, tmp);
                        }
                    }
                    OperationQueue.Enqueue(new DatabaseOperation() { id1 = id1, id2 = id2 });

                    break;
                case DatabaseMode.SemiCache:
                    break;
                default:
                    break;
            }
            return false;
        }
    }
}
