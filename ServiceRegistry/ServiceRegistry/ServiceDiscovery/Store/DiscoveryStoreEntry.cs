﻿//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using System;

namespace ServiceDiscovery.Store {

  public class DiscoveryStoreEntry {

    public string ContractFullName { get; set; }
    public int ContractMajorVersion { get; set; }
    public string InfrastructureScopeName { get; set; }

    public DateTime LastUpdateUtc { get; set; }

    public DiscoveryStoreUrlEntry[] UrlEntries { get; set; } = {};
  }

  public class DiscoveryStoreUrlEntry {

    public string Url { get; set; }

    public int AvailabilityState { get; set; } = 0;

  }

 }
