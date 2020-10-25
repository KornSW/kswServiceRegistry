//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServiceDiscovery.Cache {

  public class FileBasedDiscoveryCache : IDiscoveryCache {

    private DirectoryInfo _CacheFolder;

    public FileBasedDiscoveryCache(string cacheFolderFullName) {
      _CacheFolder = new DirectoryInfo(Path.GetFullPath(cacheFolderFullName));
    }

    public DiscoveryCacheEntry[] LoadAllEntries()  {
      List<DiscoveryCacheEntry> allEntries = new List<DiscoveryCacheEntry>();

      foreach (DirectoryInfo infrastructureScopeDirectory in _CacheFolder.GetDirectories()) {
        string infrastructureScopeName = infrastructureScopeDirectory.Name;

        foreach (FileInfo file in infrastructureScopeDirectory.GetFiles("*.*")) {
          string contractFullName = Path.GetFileNameWithoutExtension(file.Name);

          Int32 contractMajorVersion = -1;
          if (Int32.TryParse(Path.GetExtension(file.Name).Substring(1), out contractMajorVersion)) {
            string serializedEntry = File.ReadAllText(file.FullName, Encoding.UTF8);
            DiscoveryCacheEntry deserializedEntry = JsonConvert.DeserializeObject<DiscoveryCacheEntry>(serializedEntry);
            allEntries.Add(deserializedEntry);
          }

        }
      }

      return allEntries.ToArray();
    }

    public bool TryLoadEntry(string contractFullName, int contractMajorVersion, string infrastructureScopeName, ref DiscoveryCacheEntry loadedEntry) {

      DirectoryInfo infrastructureScopeDirectory = new DirectoryInfo(Path.Combine(_CacheFolder.FullName, infrastructureScopeName));
      if (!infrastructureScopeDirectory.Exists) {
        return false;
      }

      string expectedFileFullName = Path.Combine(infrastructureScopeDirectory.FullName, contractFullName + ".v" + contractMajorVersion.ToString());
      if (!File.Exists(expectedFileFullName)) {
        return false;
      }

      string serializedEntry = File.ReadAllText(expectedFileFullName, Encoding.UTF8);
      loadedEntry = JsonConvert.DeserializeObject<DiscoveryCacheEntry>(serializedEntry);

      return true;
    }

    public void RemoveEntry(string contractFullName, int contractMajorVersion, string infrastructureScopeName)    {
      
      DirectoryInfo infrastructureScopeDirectory = new DirectoryInfo(Path.Combine(_CacheFolder.FullName, infrastructureScopeName));
      if (!infrastructureScopeDirectory.Exists) {
        return;
      }

      string expectedFileFullName = Path.Combine(infrastructureScopeDirectory.FullName, contractFullName + ".v" + contractMajorVersion.ToString());
      if (!File.Exists(expectedFileFullName)) {
        return;
      }

      File.Delete(expectedFileFullName);
    }

    public void SaveEntry(DiscoveryCacheEntry entry)
    {

      DirectoryInfo infrastructureScopeDirectory = new DirectoryInfo(Path.Combine(_CacheFolder.FullName, entry.InfrastructureScopeName));
      if (!infrastructureScopeDirectory.Exists) {
        infrastructureScopeDirectory.Create();
      }

      string serializedEntry = JsonConvert.SerializeObject(entry);
      string targetFileFullName = Path.Combine(infrastructureScopeDirectory.FullName, entry.ContractFullName + ".v" + entry.ContractMajorVersion.ToString());

      File.WriteAllText(targetFileFullName, serializedEntry, Encoding.UTF8);

    }
  }
}
