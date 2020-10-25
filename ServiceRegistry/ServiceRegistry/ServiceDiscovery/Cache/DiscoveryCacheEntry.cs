//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using System;

namespace ServiceDiscovery.Cache {

  public class DiscoveryCacheEntry {

    public string ContractFullName { get; set; }
    public int ContractMajorVersion { get; set; }
    public string InfrastructureScopeName { get; set; }

    public DateTime LastUpdateUtc { get; set; }

    public string[] UrlEntries { get; set; } = {};
  }

}

