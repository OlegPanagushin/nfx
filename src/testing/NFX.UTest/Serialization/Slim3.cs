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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using NFX.Scripting;

using NFX;
using NFX.Collections;
using NFX.Serialization.Slim;
using NFX.Serialization.JSON;

namespace NFX.UTest.Serialization
{
  [Runnable]
  public class Slim3
  {

    [Run]
    public void NLS_Root()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new NLSMap("eng{n='name' d='description'} rus{n='имя' d='описание'}".AsLaconicConfig());

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (NLSMap)s.Deserialize(ms);

        Aver.AreEqual( 2, dOut.Count);
        Aver.AreEqual( "name", dOut.Get(NLSMap.GetParts.Name, "eng"));
        Aver.AreEqual( "имя", dOut.Get(NLSMap.GetParts.Name, "rus"));

        Aver.AreEqual( "description", dOut.Get(NLSMap.GetParts.Description, "eng"));
        Aver.AreEqual( "описание", dOut.Get(NLSMap.GetParts.Description, "rus"));
      }
    }

        internal class nlsCls
        {
          public NLSMap Map;
        }

    [Run]
    public void NLS_InClass()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new nlsCls{ Map = new NLSMap("eng{n='name' d='description'} rus{n='имя' d='описание'}".AsLaconicConfig())};

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (nlsCls)s.Deserialize(ms);

        Aver.IsNotNull(dOut);

        Aver.AreEqual( 2, dOut.Map.Count);
        Aver.AreEqual( "name", dOut.Map.Get(NLSMap.GetParts.Name, "eng"));
        Aver.AreEqual( "имя", dOut.Map.Get(NLSMap.GetParts.Name, "rus"));

        Aver.AreEqual( "description", dOut.Map.Get(NLSMap.GetParts.Description, "eng"));
        Aver.AreEqual( "описание", dOut.Map.Get(NLSMap.GetParts.Description, "rus"));
      }
    }


        internal struct nlsStruct
        {
          public NLSMap Map;
        }

    [Run]
    public void NLS_InStruct()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new nlsStruct{ Map = new NLSMap("eng{n='name' d='description'} rus{n='имя' d='описание'}".AsLaconicConfig())};

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (nlsStruct)s.Deserialize(ms);

        Aver.AreEqual( 2, dOut.Map.Count);
        Aver.AreEqual( "name", dOut.Map.Get(NLSMap.GetParts.Name, "eng"));
        Aver.AreEqual( "имя", dOut.Map.Get(NLSMap.GetParts.Name, "rus"));

        Aver.AreEqual( "description", dOut.Map.Get(NLSMap.GetParts.Description, "eng"));
        Aver.AreEqual( "описание", dOut.Map.Get(NLSMap.GetParts.Description, "rus"));
      }
    }


    [Run]
    public void NLS_Array()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new NLSMap[]{
                        new NLSMap("eng{n='name' d='description'} rus{n='имя' d='описание'}".AsLaconicConfig()),
                        new NLSMap("eng{n='color' d='of product'} rus{n='zvet' d='producta'}".AsLaconicConfig()),
                        new NLSMap("eng{n='size' d='of item'} rus{n='razmer' d='tovara'}".AsLaconicConfig())
                      };


        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (NLSMap[])s.Deserialize(ms);

        Aver.IsNotNull(dOut);

        Aver.AreEqual( 3, dOut.Length);

        Aver.AreEqual( "name",        dOut[0].Get(NLSMap.GetParts.Name, "eng"));
        Aver.AreEqual( "имя",         dOut[0].Get(NLSMap.GetParts.Name, "rus"));
        Aver.AreEqual( "description", dOut[0].Get(NLSMap.GetParts.Description, "eng"));
        Aver.AreEqual( "описание",    dOut[0].Get(NLSMap.GetParts.Description, "rus"));

        Aver.AreEqual( "color",      dOut[1].Get(NLSMap.GetParts.Name, "eng"));
        Aver.AreEqual( "zvet",       dOut[1].Get(NLSMap.GetParts.Name, "rus"));
        Aver.AreEqual( "of product", dOut[1].Get(NLSMap.GetParts.Description, "eng"));
        Aver.AreEqual( "producta",   dOut[1].Get(NLSMap.GetParts.Description, "rus"));

        Aver.AreEqual( "size",    dOut[2].Get(NLSMap.GetParts.Name, "eng"));
        Aver.AreEqual( "razmer",  dOut[2].Get(NLSMap.GetParts.Name, "rus"));
        Aver.AreEqual( "of item", dOut[2].Get(NLSMap.GetParts.Description, "eng"));
        Aver.AreEqual( "tovara",  dOut[2].Get(NLSMap.GetParts.Description, "rus"));
      }
    }



    [Run]
    public void StringMap_Sensitive()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new StringMap
        {
          {"a", "Alex"},
          {"b", "Boris"}
        };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (StringMap)s.Deserialize(ms);

        Aver.IsNotNull(dOut);

        Aver.IsTrue( dOut.CaseSensitive );
        Aver.AreEqual( 2, dOut.Count);
        Aver.AreEqual( "Alex", dOut["a"]);
        Aver.AreEqual( null, dOut["A"]);

        Aver.AreEqual( "Boris", dOut["b"]);
        Aver.AreEqual( null, dOut["B"]);
      }
    }

    [Run]
    public void StringMap_Insensitive()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new StringMap(false)
        {
          {"a", "Alex"},
          {"b", "Boris"}
        };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (StringMap)s.Deserialize(ms);

        Aver.IsNotNull(dOut);

        Aver.IsFalse( dOut.CaseSensitive );
        Aver.AreEqual( 2, dOut.Count);
        Aver.AreEqual( "Alex", dOut["a"]);
        Aver.AreEqual( "Alex", dOut["A"]);

        Aver.AreEqual( "Boris", dOut["b"]);
        Aver.AreEqual( "Boris", dOut["B"]);
      }
    }


        internal class stringMapCls
        {
          public StringMap Map1;
          public StringMap Map2;
        }


    [Run]
    public void StringMap_One_InClass()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new stringMapCls
        {
          Map1 = new StringMap(false)
          {
            {"a", "Alex"},
            {"b", "Boris"}
          }
        };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (stringMapCls)s.Deserialize(ms);

        Aver.IsNotNull(dOut);
        Aver.IsNotNull(dOut.Map1);
        Aver.IsNull(dOut.Map2);

        Aver.IsFalse( dOut.Map1.CaseSensitive );
        Aver.AreEqual( 2, dOut.Map1.Count);
        Aver.AreEqual( "Alex", dOut.Map1["a"]);
        Aver.AreEqual( "Alex", dOut.Map1["A"]);

        Aver.AreEqual( "Boris", dOut.Map1["b"]);
        Aver.AreEqual( "Boris", dOut.Map1["B"]);
      }
    }


    [Run]
    public void StringMap_TwoRefOne_InClass()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new stringMapCls
        {
          Map1 = new StringMap(false)
          {
            {"a", "Alex"},
            {"b", "Boris"}
          }
        };
        dIn.Map2 = dIn.Map1; //SAME REF!

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (stringMapCls)s.Deserialize(ms);

        Aver.IsNotNull(dOut);
        Aver.IsNotNull(dOut.Map1);
        Aver.IsNotNull(dOut.Map2);

        Aver.IsTrue( object.ReferenceEquals( dOut.Map1, dOut.Map2 ));//IMPORTANT!

        Aver.IsFalse( dOut.Map1.CaseSensitive );
        Aver.AreEqual( 2, dOut.Map1.Count);
        Aver.AreEqual( "Alex", dOut.Map1["a"]);
        Aver.AreEqual( "Alex", dOut.Map1["A"]);

        Aver.AreEqual( "Boris", dOut.Map1["b"]);
        Aver.AreEqual( "Boris", dOut.Map1["B"]);
      }
    }


    [Run("cnt=10")]
    [Run("cnt=210")]
    [Run("cnt=3240")]
    [Run("cnt=128000")]
    [Run("cnt=512000")]
    [Run("cnt=1512000")]
    public void StringMap_WideCharsLong(int cnt)
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var original1 = new string('久', cnt);
        var original2 = "就是巴尼宝贝儿吧，俺说。有什么怪事儿或是好事儿吗？ когда американские авианосцы 'Уинсон' и 'Мидуэй' приблизились 지구상의　３대 we have solved the problem";

        var dIn = new stringMapCls
        {
          Map1 = new StringMap(false)
          {
            {"a", original1},
            {"b", original2}
          }
        };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (stringMapCls)s.Deserialize(ms);

        Aver.IsNotNull(dOut);
        Aver.IsNotNull(dOut.Map1);
        Aver.IsNull(dOut.Map2);

        Aver.IsFalse( dOut.Map1.CaseSensitive );
        Aver.AreEqual( 2, dOut.Map1.Count);


        Aver.AreEqual( original1, dOut.Map1["a"]);
        Aver.AreEqual( original1, dOut.Map1["A"]);

        Aver.AreEqual( original2, dOut.Map1["b"]);
        Aver.AreEqual( original2, dOut.Map1["B"]);
      }
    }

    [Run]
    public void StringManyLanguages()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var original =@"
        外国語の学習と教授

Language Learning and Teaching

Изучение и обучение иностранных языков

Tere Daaheng Aneng Karimah

語文教學・语文教学

Enseñanza y estudio de idiomas

Изучаване и Преподаване на Чужди Езици

ქართული ენის შესწავლა და სწავლება

'læŋɡwidʒ 'lɘr:niŋ ænd 'ti:tʃiŋ

Lus kawm thaib qhia

Ngôn Ngữ, Sự học,

‭‫ללמוד וללמד את השֵפה

L'enseignement et l'étude des langues

말배우기와 가르치기

Nauka języków obcych

Γλωσσική Εκμὰθηση και Διδασκαλία

‭‫ﺗﺪﺭﯾﺲ ﻭ ﯾﺎﺩﮔﯿﺮﯼ ﺯﺑﺎﻥ

Sprachlernen und -lehren

‭‫ﺗﻌﻠﻢ ﻭﺗﺪﺭﻳﺲ ﺍﻟﻌﺮﺑﻴﺔ

เรียนและสอนภาษา";


        s.Serialize(ms, original);
        ms.Seek(0, SeekOrigin.Begin);

        var got = s.Deserialize(ms) as string;

        Aver.IsNotNull(got);
        Aver.AreEqual( original, got);
      }
    }




  }
}
