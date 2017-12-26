/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
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

using NFX;
using NFX.Scripting;
using NFX.DataAccess.Distributed;

namespace NFX.UTest
{
  [Runnable(TRUN.BASE)]
  public class ELinkTest
  {
      [Run]
      [Aver.Throws(typeof(NFXException), "checksum does not match")]
      public void Exception_CSUM1()
      {
          Aver.AreEqual( new GDID(7, 12, 0), new ELink( "TUSHEK-TRTV" ).GDID );
          new ELink( "TUSHEK-TOTV" ); //exception
      }

      [Run]
      [Aver.Throws(typeof(NFXException), "checksum does not match")]
      public void Exception_CSUM2()
      {
          Aver.AreEqual( new GDID(1490109883, 12, 9831762982682), new ELink( "CDHOAL-OKHIRAGNAFIPOMEP-UBBR" ).GDID );

          new ELink( "CDHOAL-OKHIRAGNAFIPOMEP-UBBA" );
      }

      [Run]
      [Aver.Throws(typeof(NFXException), "checksum does not match")]
      public void Exception_CSUM3()
      {
          Aver.AreEqual( new GDID(1490109883, 12, 9831762982682), new ELink( "CDHOAL-OKHIRAGNAFIPOMEP-UBBR" ).GDID );

          new ELink( "CDHOAL-OKHIREGNAFIPOMEP-UBBR" );
      }

      [Run]
      [Aver.Throws(typeof(NFXException), "checksum does not match")]
      public void Exception_CSUM4()
      {
          Aver.AreEqual( new GDID(1490109883, 12, 9831762982682), new ELink( "KLEMIK-SHENOGEKJAABSUHM-LSLA" ).GDID );

          new ELink( "KLEMIK-SAENOGEKJAABSUHM-LSLA" );
      }


      [Run]
      [Aver.Throws(typeof(NFXException), "invalid character data length")]
      public void Exception_CharLength()
      {
          //PYYEOP-ABYOIWUFOBSHAMNE-GOOX
          new ELink( "PYYEOP-ABYOIWUFOBSHAMNE-GO" );
      }

      [Run]
      [Aver.Throws(typeof(NFXException), "an invalid combination")]
      public void Exception_InvalidCombination()
      {
          //PYYEOP-ABYOIWUFOBSHAMNE-GOOX
          new ELink( "PYYEOP-XXYOIWUFOBSHAMNE-GOOX" ); //"WOAC-XOXOXOXOXOXOXOXO" is the original
      }



      [Run]
      public void Encode_Min()
      {
          var lnk = new ELink(0, null);
          lnk.Encode(1);
          Aver.AreEqual("AIAIAJ-AJ", lnk.Link);
      }

      [Run]
      public void Encode_Max()
      {
          var lnk = new ELink(ulong.MaxValue, null);
          lnk.Encode(1);
          Aver.AreEqual("CHBRHM-VIVIVIVI-VIVIVIVI", lnk.Link);
      }


      [Run]
      public void Encode_Decode_Min()
      {
          var lnk = new ELink(0, null);
          lnk.Encode(1);

          var lnk2 = new ELink( lnk.Link );
          Aver.AreEqual(lnk.ID, lnk2.ID);
      }


      [Run]
      public void Encode_Decode_Max()
      {
          var lnk = new ELink(ulong.MaxValue, null);
          lnk.Encode(1);

          var lnk2 = new ELink( lnk.Link );
          Aver.AreEqual(lnk.ID, lnk2.ID);
      }



      [Run]
      public void Decode_Various_Formatting()
      {
        Aver.AreEqual(123ul, new ELink("AIAIIW-IW").ID);
        Aver.AreEqual(123ul, new ELink("AIAIIWIW").ID);
        Aver.AreEqual(123ul, new ELink("aiaiiw-iw").ID);
        Aver.AreEqual(123ul, new ELink("AIAIIW IW").ID);
        Aver.AreEqual(123ul, new ELink("aiaiiwiw").ID);
        Aver.AreEqual(123ul, new ELink("ai a i i w iw").ID);
        Aver.AreEqual(123ul, new ELink("ai-a-i-i-w-iw").ID);
        Aver.AreEqual(123ul, new ELink("aIaIIWiw").ID);
        Aver.AreEqual(123ul, new ELink("aIa              IIWiw").ID);
      }



      [Run]
      public void Encode_Decode_MetaEven()
      {
          var lnk = new ELink(1, new byte[]{0x01, 0xfe, 0xda, 0x5});
          lnk.Encode(1);
          var lnk2 = new ELink( lnk.Link );
          Aver.AreEqual(lnk.ID, lnk2.ID);
          Aver.AreEqual(0x01, lnk2.Metadata[0]);
          Aver.AreEqual(0xfe, lnk2.Metadata[1]);
          Aver.AreEqual(0xda, lnk2.Metadata[2]);
          Aver.AreEqual(0x05, lnk2.Metadata[3]);
      }

      [Run]
      public void Encode_Decode_MetaOdd()
      {
          var lnk = new ELink(1, new byte[]{0x01, 0xfe, 0xda, 0x5, 0x07});
          lnk.Encode(1);

          var lnk2 = new ELink( lnk.Link );
          Aver.AreEqual(lnk.ID, lnk2.ID);
          Aver.AreEqual(0x01, lnk2.Metadata[0]);
          Aver.AreEqual(0xfe, lnk2.Metadata[1]);
          Aver.AreEqual(0xda, lnk2.Metadata[2]);
          Aver.AreEqual(0x05, lnk2.Metadata[3]);
          Aver.AreEqual(0x07, lnk2.Metadata[4]);
      }


      [Run]
      public void Encode_Decode_16_Seeds()
      {
          var lnk = new ELink(4999666333111, null);

          for(int seed = 0; seed < 16; seed++)
          {
              lnk.Encode((byte)seed);
      Console.WriteLine("{0}  {1} -> {2}".Args(seed, lnk.Link, lnk.GDID));
              var lnk2 = new ELink( lnk.Link );
              Aver.AreEqual(lnk.ID, lnk2.ID);
          }
      }


      [Run]
      public void Encode_Decode_All_Range()
      {
          long total = 0;

          ulong step = ulong.MaxValue / 10000;
          ulong inc = 1;
          for(ulong span = 0; span < ulong.MaxValue - step; span +=  inc)
          {
              System.Threading.Tasks.Parallel.For
              (0, 500,
                  (i)=>
                  {
                          ulong id = span + (ulong)i;
                          var lnk = new ELink(id, null);
                          var lnk2 = new ELink( lnk.Link );
                          Aver.AreEqual(lnk.ID, lnk2.ID);
                          System.Threading.Interlocked.Increment(ref total);
                  }
              );

              if (total % 5000 == 0)
                  Console.WriteLine("Processed {0} links = {1:P} Range {2}".Args(total, span / (double)ulong.MaxValue, span));
              if (inc>=step)
              inc = step;
              else
              inc *= 2;
          }

      }


      [Run]
      public void GDID_EncodeDecode_1()
      {
          var lnk = new ELink( new GDID(0, 7, 10678159678), null );
          Console.WriteLine("ELink {0} with GDID and no extra metadata: {1}".Args(lnk.ID, lnk.Link));


          var gdid = lnk.GDID;
          Aver.AreEqual(0ul, gdid.Era);
          Aver.AreEqual(7, gdid.Authority);
          Aver.AreEqual(10678159678ul, gdid.Counter);

              lnk = new ELink( new GDID(230, 0xf, 67123456), null );
              Console.WriteLine("ELink {0} with GDID and no extra metadata: {1}".Args(lnk.ID, lnk.Link));


              gdid = lnk.GDID;
              Aver.AreEqual(230ul, gdid.Era);
              Aver.AreEqual(15, gdid.Authority);
              Aver.AreEqual(67123456ul, gdid.Counter);

          lnk = new ELink( new GDID(0, 2, 123000), null );
          Console.WriteLine("ELink {0} with GDID and no extra metadata: {1}".Args(lnk.ID, lnk.Link));

          gdid = lnk.GDID;
          Aver.AreEqual(2, gdid.Authority);
          Aver.AreEqual(123000ul, gdid.Counter);

      }

      [Run]
      public void GDID_EncodeDecode_2()
      {
          var lnk = new ELink( new GDID(0, 3, 1678), new byte[] {1,2,3,4,5} );

          var gdid = lnk.GDID;
          Aver.AreEqual(0ul, gdid.Era);
          Aver.AreEqual(3, gdid.Authority);
          Aver.AreEqual(1678ul, gdid.Counter);
          Aver.IsNotNull( lnk.Metadata);
          Aver.AreEqual(5, lnk.Metadata.Length);

          Aver.AreEqual(1, lnk.Metadata[0]);
          Aver.AreEqual(2, lnk.Metadata[1]);
          Aver.AreEqual(3, lnk.Metadata[2]);
          Aver.AreEqual(4, lnk.Metadata[3]);
          Aver.AreEqual(5, lnk.Metadata[4]);

      }

      [Run]
      public void GDID_EncodeDecode_3()
      {
          var src = new ELink( new GDID(567333, 3, 167898777), new byte[] {1,2,3,4,5} );
          Console.WriteLine("ELink with GDID and 5 bytes metadata: "+src.Link);
          var lnk = new ELink( src.Link );

          var gdid = lnk.GDID;
          Aver.AreEqual(567333ul, gdid.Era);
          Aver.AreEqual(3, gdid.Authority);
          Aver.AreEqual(167898777ul, gdid.Counter);
          Aver.IsNotNull( lnk.Metadata);
          Aver.AreEqual(5, lnk.Metadata.Length);

          Aver.AreEqual(1, lnk.Metadata[0]);
          Aver.AreEqual(2, lnk.Metadata[1]);
          Aver.AreEqual(3, lnk.Metadata[2]);
          Aver.AreEqual(4, lnk.Metadata[3]);
          Aver.AreEqual(5, lnk.Metadata[4]);

      }


      [Run]
      public void GDID_EncodeDecode_4()
      {
//CHAGVU-ANHTUGIFNU-TVWETOUF-JOJKJU
          var src = new ELink( new GDID(27, 15, 200100999666333000), new byte[] {(byte)'a',(byte)'b',(byte)'c'} );
          Console.WriteLine("{0} -> {1} + 'abc'".Args(src.Link, src.GDID));
          var lnk = new ELink( src.Link );

          var gdid = lnk.GDID;
          Aver.AreEqual(27ul, gdid.Era);
          Aver.AreEqual(15, gdid.Authority);
          Aver.AreEqual(200100999666333000ul, gdid.Counter);
          Aver.IsNotNull( lnk.Metadata);
          Aver.AreEqual(3, lnk.Metadata.Length);

          Aver.AreEqual((byte)'a', lnk.Metadata[0]);
          Aver.AreEqual((byte)'b', lnk.Metadata[1]);
          Aver.AreEqual((byte)'c', lnk.Metadata[2]);

      }



  }

}
