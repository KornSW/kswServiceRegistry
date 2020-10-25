//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using ServiceDiscovery;
using System;
using System.IO;

public class ServiceRegistry : FileBasedServiceRegistry {

  private static string GetStorageFolderFullName() {
    string appDataFolderFullName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    string storageFolderFullName = Path.Combine(appDataFolderFullName, "kswServiceRegistry","DB");
    return storageFolderFullName;
  }

  public ServiceRegistry(string storageFolderFullName) : base(GetStorageFolderFullName()) {
  }

}
