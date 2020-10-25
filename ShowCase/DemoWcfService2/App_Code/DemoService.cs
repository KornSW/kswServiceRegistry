//  +------------------------------------------------------------------------+
//  ¦ this file is part of an open-source solution which is originated here: ¦
//  ¦ https://github.com/KornSW/kswServiceRegistry                           ¦
//  ¦ the removal of this notice is prohibited by the author!                ¦
//  +------------------------------------------------------------------------+

using Demo.V1;

public class DemoService : Demo.V2.IDemoService, Demo.V1.IDemoService {

  string Demo.V1.IDemoService.Foo(string arg) {
    return this.Foo(arg, 1);
  }

  public string Foo(string arg, int count) {
    for (int i = 1; i <= count; i++) {
      arg = arg + " Foo";
    }
    return arg;
  }
  public string Bar() {
    return "bar";
  }

}
