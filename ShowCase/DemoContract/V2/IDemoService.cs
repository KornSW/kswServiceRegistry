//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Demo.V2 {

  [ServiceContract]
  public interface IDemoService {

    [OperationContract, WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json,ResponseFormat = WebMessageFormat.Json)]
    string Foo(string arg, int count);

    [OperationContract, WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    string Bar();

  }

}
