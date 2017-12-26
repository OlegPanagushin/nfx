﻿/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/


using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NFX.Scripting;


using NFX.Environment;
using static NFX.Aver.ThrowsAttribute;

namespace NFX.UTest.Config
{

    [Runnable(TRUN.BASE)]
    public class VariableEvaluation
    {
        static string xml =
@"<root>

     <varEscaped>$(###)This is not var: $(../AAA)</varEscaped>
     <varIncomplete1>$()</varIncomplete1>
     <varIncomplete2>$(</varIncomplete2>

   <vars>

     <var1>val1</var1>
     <var2>$(../var1)</var2>

     <path1 value='c:\logs\' />
     <path2 value='\critical' />

     <many>
        <a value='1' age='18'>1</a>
        <a value='2' age='25'>2</a>
     </many>

     <var3>$(../var4)</var3>
     <var4>$(../var3)</var4>

     <var5>$(../var6)</var5>
     <var6>$(../var7)</var6>
     <var7>$(../var1)$(../var3)$(../var2)</var7>
   </vars>


   <MyClass>
    <data pvt-name='private'
          prot-name='protected'
          pub-name='public'
          age='99'>

          <extra
            enum='B'
            when='05/12/1982'
            cycle='$(/vars/var5)'
            >

            <fuzzy>true</fuzzy>
            <jazzy></jazzy>

          </extra>
    </data>
  </MyClass>

  <this name='$(/vars/var1)' text='This happened on $(../MyClass/data/extra/$when) date' />

  <logger location='$(/vars/path1/$value)$(@/vars/path2/$value)'/>

  <optional>$(/non-existent)</optional>
  <required>$(!/non-existent)</required>

  <env1>$(~A)</env1>
  <env2>$(~A)+$(~B)</env2>
  <env3>$(~A)$(@~B)</env3>


 </root>
";


        [Run]
        public void EscapedVar()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("This is not var: $(../AAA)", conf.Root.Navigate("/varEscaped").Value );
        }

        [Run]
        public void IncompleteVars()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("", conf.Root.Navigate("/varIncomplete1").Value );
          Aver.AreEqual("$(", conf.Root.Navigate("/varIncomplete2").Value );
        }


        [Aver.Throws(typeof(ConfigException), Message="not a section node", MsgMatch=MatchType.Contains)]
        [Run]
        public void BadPathWithAttrAttr()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Aver.IsFalse( conf.Root.Navigate("/vars/path1/$value/$kaka").Exists );
        }


        [Run]
        public void PathWithPipes()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("\\critical", conf.Root.Navigate("/vars/paZZ1/$value|/vars/path2/$value").Value);
        }

        [Run]
        public void PathWithSectionIndexer()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("\\critical", conf.Root.Navigate("/vars/[3]/$value").Value);
        }

        [Run]
        public void PathWithAttributeIndexer()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("\\critical", conf.Root.Navigate("/vars/path2/$[0]").Value);
        }


        [Run]
        public void PathWithValueIndexer()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual(1, conf.Root.Navigate("/vars/many/a[1]").ValueAsInt());
          Aver.AreEqual(2, conf.Root.Navigate("/vars/many/a[2]").ValueAsInt());
        }

        [Run]
        public void PathWithAttributeValueIndexer()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual(1, conf.Root.Navigate("/vars/many/a[age=18]").ValueAsInt());
          Aver.AreEqual(2, conf.Root.Navigate("/vars/many/a[age=25]").ValueAsInt());
        }



        [Aver.Throws(typeof(ConfigException), Message="syntax", MsgMatch=MatchType.Contains)]
        [Run]
        public void PathWithBadIndexerSyntax1()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          conf.Root.Navigate("/]/$value");
        }

        [Aver.Throws(typeof(ConfigException), Message="syntax", MsgMatch=MatchType.Contains)]
        [Run]
        public void PathWithBadIndexerSyntax2()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          conf.Root.Navigate("/aaa]/$value");
        }



        [Aver.Throws(typeof(ConfigException), Message="syntax", MsgMatch=MatchType.Contains)]
        [Run]
        public void PathWithBadIndexerSyntax3()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          conf.Root.Navigate("/[/$value");
        }




        [Run]
        public void TestNavigationinVarNames()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);


          Aver.AreEqual("val1", conf.Root["vars"]["var1"].Value);
          Aver.AreEqual("val1", conf.Root["vars"]["var1"].VerbatimValue);

          Aver.AreEqual("val1", conf.Root["vars"]["var2"].Value);
          Aver.AreEqual("$(../var1)", conf.Root["vars"]["var2"].VerbatimValue);


          Aver.AreEqual("val1", conf.Root["this"].AttrByName("name").Value);
          Aver.AreEqual("$(/vars/var1)", conf.Root["this"].AttrByName("name").VerbatimValue);
          Aver.AreEqual("$(/vars/var1)", conf.Root["this"].AttrByName("name").ValueAsString(verbatim: true));

          Aver.AreEqual("This happened on 05/12/1982 date", conf.Root["this"].AttrByName("text").Value);

          Aver.AreEqual(@"c:\logs\critical", conf.Root["logger"].AttrByName("location").Value);

        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void Recursive1()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("$(../var4)", conf.Root["vars"]["var3"].VerbatimValue);//no exception
          var v = conf.Root["vars"]["var3"].Value;
        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void Recursive2()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("$(../var3)", conf.Root["vars"]["var4"].VerbatimValue);//no exception
          var v = conf.Root["vars"]["var4"].Value;
        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void Recursive3Transitive()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var attr = conf.Root["MyClass"]["data"]["extra"].AttrByName("cycle");

          var v1 = attr.VerbatimValue;//no exception
          var v2 = attr.Value;//exception
        }


        [Run]
        public void Recursive4StackCleanup()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var attr = conf.Root["MyClass"]["data"]["extra"].AttrByName("cycle");

          try
          {
           var v2 = attr.Value;//exception
          }
          catch(Exception error)
          {
           Console.WriteLine("Expected and got: "+error.Message);
          }

          //after exception, stack should cleanup and work again as expected
          Aver.AreEqual("val1", conf.Root["vars"]["var1"].Value);
        }


        [Run]
        public void Optional()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual(true, string.IsNullOrEmpty(conf.Root["optional"].Value));
        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void Required()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          var v = conf.Root["required"].Value;
        }



        [Run]
        public void EnvVars1()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);
          conf.EnvironmentVarResolver = new MyVars();

           Aver.AreEqual("1", conf.Root["env1"].Value);
           Aver.AreEqual("1+2", conf.Root["env2"].Value);
           Aver.AreEqual(@"1\2", conf.Root["env3"].Value);

        }


        [Run]
        public void EvalFromString_1()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

           Aver.AreEqual("Hello val1", "Hello $(vars/var1)".EvaluateVarsInConfigScope(conf));
           Aver.AreEqual("Hello val1", "Hello $(vars/var1)".EvaluateVarsInXMLConfigScope(xml));
           Aver.AreEqual("Hello 123 suslik!", "Hello $(/$v) suslik!".EvaluateVarsInXMLConfigScope("<a v='123'> </a>"));
        }

        [Run]
        public void EvalFromString_2_manysame()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

           Aver.AreEqual("Hello val1val1val1", "Hello $(vars/var1)$(vars/var1)$(vars/var1)".EvaluateVarsInConfigScope(conf));
        }



        [Run]
        public void EvalFromStringWithEnvVarResolver()
        {
           Aver.AreEqual("Time is 01/18/1901 2:03PM", "Time is $(~C)".EvaluateVars( new MyVars()));
        }


        [Run]
        public void EvalFromStringWithEnvVarInline()
        {
           Aver.AreEqual("Hello, your age is 123", "$(~GreEtInG), your age is $(~AGE)".EvaluateVars( new Vars(new VarsDictionary {
                            {"Greeting", "Hello"},
                            {"Age", "123"}
           })));
        }


        [Run]
        public void EvalFromStringWithEnvVarAndMacro()
        {
           Aver.AreEqual("Time is 01/1901", "Time is $(~C::as-dateTime fmt=\"{0:MM/yyyy}\")".EvaluateVars( new MyVars()));
        }

        [Run]
        public void EvalFromStringWithEnvVarAndMacro2()
        {
           Aver.AreEqual("Time is Month=01 Year=1901", "Time is $(~C::as-dateTime fmt=\"Month={0:MM} Year={0:yyyy}\")".EvaluateVars( new MyVars()));
        }


        [Run]
        public void EvalFromStringMacroDefault()
        {
           Aver.AreEqual("Value is 12 OK?", "Value is $(/dont-exist::as-int dflt=\"12\") OK?".EvaluateVars());
        }

        [Run]
        public void EvalFromStringMacroDefault2()
        {
           Aver.AreEqual("James, the value is 12 OK?",
                           "$(/$name::as-string dflt=\"James\"), the value is $(/dont-exist::as-int dflt=\"12\") OK?".EvaluateVars());
        }

        [Run]
        public void EvalFromStringMacroDefault3()
        {
           Aver.AreEqual("Mark Spenser, the value is 12 OK?",
                           "$(~name::as-string dflt=\"James\"), the value is $(/dont-exist::as-int dflt=\"12\") OK?".EvaluateVars(
                            new Vars( new VarsDictionary { {"name", "Mark Spenser"}  })
                           ));
        }

        [Run]
        public void EvalTestNowString()
        {
            Aver.AreEqual("20131012-06", "$(::now fmt=yyyyMMdd-HH value=20131012-06)".EvaluateVars());
        }

        [Run]
        public void NodePaths()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);


          Aver.AreEqual("/vars/path2", conf.Root["vars"]["path2"].RootPath);
          Aver.AreEqual("/vars/path2/$value", conf.Root["vars"]["path2"].AttrByIndex(0).RootPath);
          Aver.AreEqual("/vars/many/[0]", conf.Root["vars"]["many"][0].RootPath);
          Aver.AreEqual("/vars/many/[1]", conf.Root["vars"]["many"][1].RootPath);
          Aver.AreEqual("/vars/many/[1]/$value", conf.Root["vars"]["many"][1].AttrByIndex(0).RootPath);
        }


        [Run]
        public void NavigateBackToNodePaths()
        {
          var conf = NFX.Environment.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("/vars/path2",           conf.Root.Navigate( conf.Root["vars"]["path2"].RootPath                  ).RootPath);
          Aver.AreEqual("/vars/path2/$value",    conf.Root.Navigate( conf.Root["vars"]["path2"].AttrByIndex(0).RootPath   ).RootPath);
          Aver.AreEqual("/vars/many/[0]",        conf.Root.Navigate( conf.Root["vars"]["many"][0].RootPath                ).RootPath);
          Aver.AreEqual("/vars/many/[1]",        conf.Root.Navigate( conf.Root["vars"]["many"][1].RootPath                ).RootPath);
          Aver.AreEqual("/vars/many/[1]/$value", conf.Root.Navigate( conf.Root["vars"]["many"][1].AttrByIndex(0).RootPath ).RootPath);
        }


    }



                        class MyVars : IEnvironmentVariableResolver
                        {

                          public bool ResolveEnvironmentVariable(string name, out string value)
                          {
                            value = null;
                            if (name == "A") value = "1";
                            if (name == "B") value = "2";
                            if (name ==  "C") value = "01/18/1901 2:03PM";
                            return true;
                          }
                        }




}



