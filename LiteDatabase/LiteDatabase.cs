using LiteDatabase.CustomedCryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public readonly static Version DatabaseVersion = new Version(1, 0, 2, 0);
        public CryptographyCredential cryptographyCredential = new CryptographyCredential();
        public DirectoryInfo HomeDirectory = new DirectoryInfo("./Databases/");
        public string givenHome = "";
        public string GeneratedID = "";
        public Database(String Home="./Databases/",DatabaseMode loadMode = DatabaseMode.OnDemand,CryptographyCredential cryptographyCredential= null)
        {
            givenHome = Home;
            if (cryptographyCredential == null)
            {
                this.cryptographyCredential = new CryptographyCredential();
            }
            else
            {
                this.cryptographyCredential = cryptographyCredential;
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
                    break;
                case DatabaseMode.SemiCache:
                    break;
                default:
                    break;
            }
            return vs;
        }
        public void ClearID1(string id1)
        {
            File.WriteAllText(Path.Combine(HomeDirectory.FullName, CurrentForm,$"{id1}.lite-db"), $"#Database.Version:{DatabaseVersion.Build}\r\n#Form:{CurrentForm}\r\n#Flavor={Flavor}");
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
                            var id1=item.Name.Substring(0, item.Name.Length - ".lite-db".Length);
                            var d = LoadFile(id1);
                            data.Add(id1, d);
                        }
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
            FileInfo fi = new FileInfo(Path.Combine(FormDirectory.FullName,name + ".lite-db"));
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
                        if (isFSL != true)
                        {
                            Content += tmp;
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
                        d.Add(id2, Content);
                        Content = "";
                        id2 = "";
                    }
                }
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
                                Console.WriteLine("Cannot find:" + id2);
                                tR.Dispose();
                                throw new Exception("Cannot find:"+id1+","+id2);
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
                                    if (isFSL != true)
                                    {
                                        Content += tmp;
                                    }
                                    else
                                    {

                                        Content += "\r\n" + tmp;
                                    }
                                }
                                else
                                {
                                    isCombine = false;
                                    StopReading = true;
                                    tR.Dispose();
                                    return Content;
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
        public void Remove(string id1,string id2)
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
            File.WriteAllLines(fi.FullName, tR);
            if(LoadMode== DatabaseMode.Cache||LoadMode== DatabaseMode.SemiCache)
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
                            var cont = File.ReadAllText(fi.FullName);
                            if (cont == "")
                                File.WriteAllText(fi.FullName, $"#Database.Ver={DatabaseVersion.Build}\r\n#Form={CurrentForm}\r\n#Flavor={Flavor}\r\n#DATA:{id2}\r\n{content}\r\nDATA#\r\n");
                            else
                            {
                                File.AppendAllText(fi.FullName, $"#DATA:{id2}\r\n{content}\r\nDATA#\r\n");
                            }
                        }
                        else
                        {
                            tR.RemoveRange(index, count);
                            tR.Insert(index, $"#DATA:{id2}\r\n{content}\r\nDATA#");
                            File.WriteAllLines(fi.FullName, tR);
                        }
                    }
                    break;
                case DatabaseMode.Cache:

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
                            var cont = File.ReadAllText(fi.FullName);
                            if (cont == "")
                                File.WriteAllText(fi.FullName, $"#Database.Ver={DatabaseVersion.Build}\r\n#Form={CurrentForm}\r\n#Flavor={Flavor}\r\n#DATA:{id2}\r\n{content}\r\nDATA#\r\n");
                            else
                            {
                                File.AppendAllText(fi.FullName, $"#DATA:{id2}\r\n{content}\r\nDATA#\r\n");
                            }
                        }
                        else
                        {
                            tR.RemoveRange(index, count);
                            tR.Insert(index, $"#DATA:{id2}\r\n{content}\r\nDATA#");
                            File.WriteAllLines(fi.FullName, tR);
                        }
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
