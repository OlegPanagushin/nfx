﻿/*<FILE_LICENSE>
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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

using NFX;
using NFX.Glue;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;
using NFX.DataAccess.MongoDB;
using NFX.Scripting;
using NFX.Serialization.JSON;

namespace NFX.ITest.CRUD
{
  /// <summary>
  /// Mongo CRUD tests
  /// </summary>
  [Runnable]
  public class MongoTests : IRunHook
  {
      private const string SCRIPT_ASM = "NFX.ITest";

      private static readonly Node CONNECT_NODE = NFX.DataAccess.MongoDB.Connector.Connection.DEFAUL_LOCAL_NODE;
      private static readonly string CONNECT_STR = CONNECT_NODE.ConnectString;
      private const string DB_NAME = "nfxtest";


      private MongoDBDataStore store;

      bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
      {
         store = new MongoDBDataStore(CONNECT_STR, DB_NAME);
         store.QueryResolver.ScriptAssembly = SCRIPT_ASM;
         store.QueryResolver.RegisterHandlerLocation("NFX.ITest.CRUD.MongoSpecific, NFX.ITest");
         clearAll();
         return false;
      }

      bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
      {
         DisposableObject.DisposeAndNull(ref store);
         return false;
      }

      private void clearAll()
      {
        using (var db = NFX.DataAccess.MongoDB.Connector.MongoClient.Instance[CONNECT_NODE][DB_NAME])
        {
          foreach( var cn in db.GetCollectionNames())
           db[cn].Drop();
        }
      }

      //==========================================================================================

      [Run("skip=0   fetchBy=1")]
      [Run("skip=277 fetchBy=3")]
      [Run("skip=378 fetchBy=11")]
      [Run("skip=999 fetchBy=23")]
      [Run("skip=0   fetchBy=56")]
      [Run("skip=450 fetchBy=72")]
      [Run("skip=2   fetchBy=100")]
      public void TestFindFetchBy(int skip, int fetchBy)
      {
        const int COUNT = 1000;
        for(var i=0; i < COUNT; i++)
        {
          var row = new MyPerzon
          {
              GDID = new GDID(1, 1, (ulong)i),
              Name = "Jeka Koshmar",
              Age = i
          };

          store.Insert(row);
        }

        using (var db = NFX.DataAccess.MongoDB.Connector.MongoClient.Instance[CONNECT_NODE][DB_NAME])
        {
          var collection = db["MyPerzon"];
          var query = new NFX.DataAccess.MongoDB.Connector.Query();

          var cur = collection.Find(query, skip, fetchBy);
          using(cur)
          {
            Aver.AreEqual(cur.Count(), COUNT - skip);
          }
        }
      }

      [Run]
      public void Insert()
      {
          var row = new MyPerzon
          {
             GDID = new GDID(1, 1, 100),
             Name = "Jeka Koshmar",
             Age = 89
          };

          var affected = store.Insert(row);
          Aver.AreEqual(1, affected);
      }

      [Run]
      public void InsertAndLoad()
      {
          var row = new MyPerzon
          {
             GDID = new GDID(1, 1, 100),
             Name = "Jeka Koshmar",
             Age = 89
          };

          store.Insert(row);

          var row2 = store.LoadOneRow(new Query("CRUD.LoadPerzon", typeof(MyPerzon))
                           {
                             new Query.Param("id", row.GDID)
                           }) as MyPerzon;
          Aver.IsNotNull( row2 );

          Aver.AreEqual(row.GDID, row2.GDID);
          Aver.AreEqual(row.Name, row2.Name);
          Aver.AreEqual(row.Age,  row2.Age);
      }

      [Run]
      public void InsertAndLoadWithProjection()
      {
          var row = new MyPerzon
          {
             GDID = new GDID(1, 1, 100),
             Name = "Jeka Koshmar",
             Age = 89
          };

          store.Insert(row);

          var row2 = store.LoadOneRow(new Query("CRUD.LoadPerzonAge", typeof(MyPerzon))
                           {
                             new Query.Param("id", row.GDID)
                           }) as MyPerzon;
          Aver.IsNotNull( row2 );

          Aver.AreEqual(GDID.Zero, row2.GDID);
          Aver.AreEqual(null, row2.Name);
          Aver.AreEqual(row.Age,  row2.Age);
      }

      [Run]
      public void InsertAndLoadRowIntoDynamicRow()
      {
          var row = new MyPerzon
          {
             GDID = new GDID(2, 2, 200),
             Name = "Zalup Marsoedov",
             Age = 89
          };

          store.Insert(row);

          var row2 = store.LoadRow(new Query<DynamicRow>("CRUD.LoadPerzon")
                           {
                             new Query.Param("id", row.GDID)
                           });
          Aver.IsNotNull( row2 );

          Aver.AreEqual(row.GDID, row2["_id"].AsGDID());
          Aver.AreObjectsEqual(row.Name, row2["Name"]);
          Aver.AreObjectsEqual(row.Age,  row2["Age"]);
      }


      [Run]
      public void InsertManyAndLoadMany()
      {
          for(var i=0; i<100; i++)
          {
            var row = new MyPerzon
            {
               GDID = new GDID(1, 1, (ulong)i),
               Name = "Jeka Koshmar",
               Age = i
            };

            store.Insert(row);
          }

          var rs = store.LoadOneRowset(new Query("CRUD.LoadPerzonsInAgeSpan", typeof(MyPerzon))
                           {
                             new Query.Param("fromAge", 10),
                             new Query.Param("toAge", 15)
                           });
          Aver.IsNotNull( rs );

          Aver.AreEqual(4, rs.Count);

          Aver.AreObjectsEqual(14, rs.First()["Age"]);
          Aver.AreObjectsEqual(11, rs.Last()["Age"]);
      }


      [Run]
      public void InsertManyAndLoadCursor()
      {
          const int CNT = 1000;
          for(var i=0; i<CNT; i++)
          {
            var row = new MyData
            {
               ID = i,
               Data = "i is "+i.ToString()
            };

            store.Insert(row);
          }

          {
            var cursor = store.OpenCursor(  new Query("CRUD.LoadAllMyData", typeof(MyData))  );
            Aver.IsNotNull( cursor );
            var lst = new List<Row>();
            foreach(var row in cursor)
             lst.Add(row);

            Aver.AreEqual(CNT, lst.Count);
            Aver.IsTrue( cursor.Disposed );

            Console.WriteLine(lst[0].ToJSON());
            Console.WriteLine("..............................");
            Console.WriteLine(lst[lst.Count-1].ToJSON());

            Aver.AreObjectsEqual(0, lst[0]["ID"].AsInt());
            Aver.AreObjectsEqual(CNT-1, lst[lst.Count-1]["ID"].AsInt());
          }
 Console.WriteLine("A");
          {
            Cursor cursor;
            var lst = new List<Row>();

            using(cursor = store.OpenCursor(  new Query("CRUD.LoadAllMyData", typeof(MyData))  ))
            {
              Aver.IsNotNull( cursor );
              foreach(var row in cursor)
               lst.Add(row);

              try
              {
                foreach(var row in cursor)
                  lst.Add(row);
                Aver.Fail("Can not iterate the second time");
              }
              catch(Exception error)
              {
                Console.WriteLine("Expected and got: "+error.ToMessageWithType());
              }
            }
            Aver.AreEqual(CNT, lst.Count);
            Aver.IsTrue( cursor.Disposed );
          }
Console.WriteLine("B");
          {
            Cursor cursor;

            using(cursor = store.OpenCursor(  new Query("CRUD.LoadAllMyData", typeof(MyData))  ))
            {
              Aver.IsNotNull( cursor );
              var en = cursor.GetEnumerator();
              Aver.IsTrue(en.MoveNext());
              Aver.IsTrue(en.MoveNext());
              Aver.IsTrue(en.MoveNext());
              //Notice, We DO NOT iterate to the very end
              //... not till the end
            }
            Aver.IsTrue( cursor.Disposed );
          }
Console.WriteLine("C");
      }


      [Run]
      public void InsertUpsertUpdate()
      {
          var row = new MyPerzon
          {
              GDID = new GDID(1, 1, 1),
              Name = "Eight Eightavich",
              Age = 8
          };

          Aver.AreEqual(1, store.Insert(row) );

          var qryBetween1015 = new Query("CRUD.LoadPerzonsInAgeSpan", typeof(MyPerzon))
                           {
                             new Query.Param("fromAge", 10),
                             new Query.Param("toAge", 15)
                           };


          var rs = store.LoadOneRowset(qryBetween1015);
          Aver.IsNotNull( rs );

          Aver.AreEqual(0, rs.Count);

          row =  new MyPerzon
          {
              GDID = new GDID(1, 1, 2),
              Name = "T Twelver",
              Age = 12
          };

          Aver.AreEqual(0, store.Update(row) );//update did not find

          Aver.AreEqual(1, store.Upsert(row) );//upsert DID find

          row.Name="12-er-changed";
          Aver.AreEqual(1, store.Update(row) );//update DID find this time

          rs = store.LoadOneRowset(qryBetween1015);
          Aver.IsNotNull( rs );

          Aver.AreEqual(1, rs.Count);
          row = rs[0] as MyPerzon;
          Aver.AreEqual(new GDID(1,1,2), row.GDID);
          Aver.AreEqual("12-er-changed", row.Name);
      }


      [Run]
      public void InsertUpsert()
      {
          var row = new MyPerzon
          {
              GDID = new GDID(1, 1, 1),
              Name = "Eight Eightavich",
              Age = 8
          };

          Aver.AreEqual(1, store.Insert(row) );

          var qryBetween1015 = new Query("CRUD.LoadPerzonsInAgeSpan", typeof(MyPerzon))
                           {
                             new Query.Param("fromAge", 10),
                             new Query.Param("toAge", 15)
                           };


          var rs = store.LoadOneRowset(qryBetween1015);
          Aver.IsNotNull( rs );

          Aver.AreEqual(0, rs.Count);

          row =  new MyPerzon
          {
              GDID = new GDID(1, 1, 2),
              Name = "T Twelver",
              Age = 12
          };

          Aver.AreEqual(1, store.Insert(row) );

          rs = store.LoadOneRowset(qryBetween1015);
          Aver.IsNotNull( rs );

          Aver.AreEqual(1, rs.Count);
          var cr = rs[0];
          Aver.AreObjectsEqual("T Twelver", cr["Name"]);

          cr["Name"] = "12-er";
          store.Upsert(cr);

          rs = store.LoadOneRowset(qryBetween1015);
          Aver.IsNotNull( rs );

          Aver.AreEqual(1, rs.Count);
          row = rs[0] as MyPerzon;
          Aver.AreEqual("12-er", row.Name);
      }


      [Run]
      public void InsertDelete()
      {
          for(var i=0; i<100; i++)
          {
            var row = new MyPerzon
            {
               GDID = new GDID(1, 1, (ulong)i),
               Name = "Jeka Koshmar",
               Age = i
            };

            store.Insert(row);
          }

          var qryBetween1015 = new Query("CRUD.LoadPerzonsInAgeSpan", typeof(MyPerzon))
                           {
                             new Query.Param("fromAge", 10),
                             new Query.Param("toAge", 15)
                           };

          var rs = store.LoadOneRowset(qryBetween1015);
          Aver.IsNotNull( rs );

          Aver.AreEqual(4, rs.Count);

          Aver.AreObjectsEqual(14, rs.First()["Age"]);
          Aver.AreObjectsEqual(11, rs.Last()["Age"]);


          Aver.AreEqual(1, store.Delete(rs.First()));//DELETE!!!!

          rs = store.LoadOneRowset(qryBetween1015);
          Aver.IsNotNull( rs );

          Aver.AreEqual(3, rs.Count);

          Aver.AreObjectsEqual(13, rs.First()["Age"]);
          Aver.AreObjectsEqual(11, rs.Last()["Age"]);
      }


      [Run]
      public void Save()
      {
          var rowset = new Rowset(Schema.GetForTypedRow(typeof(MyPerzon)));
          rowset.LogChanges = true;

          for(var i=0; i<100; i++)
          {
            rowset.Insert( new MyPerzon
            {
               GDID = new GDID(1, 1, (ulong)i),
               Name = "Jeka Koshmar",
               Age = i
            });
          }

          var qryBetween5060 = new Query("CRUD.LoadPerzonsInAgeSpan", typeof(MyPerzon))
                           {
                             new Query.Param("fromAge", 50),
                             new Query.Param("toAge", 60)
                           };

          var rs = store.LoadOneRowset(qryBetween5060);
          Aver.IsNotNull( rs );

          Aver.AreEqual(0, rs.Count);

          store.Save(rowset);
          rowset.PurgeChanges();

          rs = store.LoadOneRowset(qryBetween5060);
          Aver.IsNotNull( rs );

          Aver.AreEqual(9, rs.Count);

          rowset[55]["Age"] = 900;  //falls out of query
          rowset.Update(rowset[55]);
          rowset.Delete(rowset[59]); //physically deleted
          store.Save(rowset);

          rs = store.LoadOneRowset(qryBetween5060);
          Aver.IsNotNull( rs );

          Aver.AreEqual(7, rs.Count);
          Aver.AreObjectsEqual(58, rs.First()["Age"]);
          Aver.AreObjectsEqual(51, rs.Last()["Age"]);
      }

      [Run]
      public void GetSchema_ROW_JSON_ROW()
      {
         var data = new byte[] { 0x00, 0x79, 0x14 };
         var row =
           new MyPerzon
            {
               GDID = new GDID(1, 1, 980),
               Name = "Lenin Grib",
               Age = 100,
               Data = data
            };

         store.Insert(row);

         var qry = new Query("CRUD.LoadPerzon", typeof(MyPerzon))
                           {
                             new Query.Param("id", new GDID(1,1,980))
                           };

         var schema = store.GetSchema(qry);

         Aver.IsNotNull(schema);
         Aver.AreEqual(4, schema.FieldCount);

         Aver.AreEqual(0, schema["_id"].Order);
         Aver.AreEqual(1, schema["Name"].Order);
         Aver.AreEqual(2, schema["Age"].Order);
         Aver.AreEqual(3, schema["Data"].Order);

         var row2 = new DynamicRow(schema);//Notice: We are creating dynamic row with schema taken from Mongo

         row2["_id"] = new GDID(10,10,10);
         row2["Name"] = "Kozloff";
         row2["Age"] = "199";
         row2["Data"] = data;

         var json = row2.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);
         Console.WriteLine(json);

         var dyn = json.JSONToDynamic();

         Aver.AreEqual(4, dyn.Data.Count);
         Aver.AreEqual("10:10:10", dyn._id);
         Aver.AreEqual("Kozloff", dyn.Name);
         Aver.AreEqual("199", dyn.Age);
         //todo: how to check dynamic row with 'Data' name? dyn.Data is the collection of all kvp ((JSONDataMap)dyn.Data)["Data"] is JSONDataArray
         //Aver.AreEqual(data, dyn.Data);
      }


      [Run]
      public void Count()
      {
        for(var i=0; i<100; i++)
          {
            var row = new MyPerzon
            {
               GDID = new GDID(1, 1, (ulong)i),
               Name = "Jeka Koshmar",
               Age = i
            };

            store.Insert(row);
          }

          //Note!
          // this query is implemented in C# code
          var rs = store.LoadOneRowset(new Query("CountPerzons")
                           {
                             new Query.Param("fromAge", 10),
                             new Query.Param("toAge", 90)
                           });
          Aver.IsNotNull( rs );

          Aver.AreEqual(1, rs.Count);

          Aver.AreObjectsEqual(79, rs[0]["Count"].AsInt());
      }

        [Run]
        public void InsertWithPredicate()
        {
            var person = new MyPerzon
            {
                GDID = new GDID(1, 1, 1),
                Name = "Jack London",
                Age = 23
            };

            var affected = store.Insert(person, (r, k, f) => f.Name != "Age");
            Aver.AreEqual(1, affected);

            var query = new Query<MyPerzon>("CRUD.LoadPerzon")
                           {
                             new Query.Param("id", person.GDID)
                           };
            var persisted = store.LoadRow(query);
            Aver.AreEqual(person.Name, persisted.Name);
            Aver.AreEqual(0, persisted.Age);
        }

        [Run]
        public void UpdateWithPredicate()
        {
            var person = new MyPerzon
            {
                GDID = new GDID(1, 1, 1),
                Name = "Jack London",
                Age = 23
            };

            store.Insert(person);
            var query = new Query<MyPerzon>("CRUD.LoadPerzon")
                           {
                             new Query.Param("id", person.GDID)
                           };
            var persisted = store.LoadRow(query);
            persisted.Name = "Ivan";
            persisted.Age = 56;

            var affected = store.Update(persisted, null, (r, k, f) => f.Name != "Name");
            var updated = store.LoadRow(query);

            Aver.AreEqual(1, affected);
            Aver.AreEqual(person.Name, updated.Name);
            Aver.AreEqual(persisted.Age, updated.Age);
        }

        [Run]
        public void UpsertWithPredicate()
        {
            var person = new MyPerzon
            {
                GDID = new GDID(1, 1, 1),
                Name = "Jack London",
                Age = 23
            };

            store.Insert(person);
            var query = new Query<MyPerzon>("CRUD.LoadPerzon")
                           {
                             new Query.Param("id", person.GDID)
                           };
            var persisted = store.LoadRow(query);
            persisted.Name = "Ivan";
            persisted.Age = 56;

            var affected = store.Upsert(persisted, (r, k, f) => f.Name != "Name");
            var upserted = store.LoadRow(query);

            Aver.AreEqual(1, affected);
            Aver.AreEqual(null, upserted.Name);
            Aver.AreEqual(persisted.Age, upserted.Age);
        }

        [Run]
        public void ExecuteWithoutFetch_InsertRows()
        {
            var id1 = new GDID(0, 0, 1);
            var id2 = new GDID(0, 0, 2);
            var id3 = new GDID(0, 0, 3);
            var data = new byte[] { 0x00, 0x79, 0x14 };
            var query = new Query<MyPerzon>("CRUD.InsertPerzons")
            {
                new Query.Param("id1", id1),
                new Query.Param("id2", id2),
                new Query.Param("id3", id3),
                new Query.Param("data", data)
            };

            var affected = store.ExecuteWithoutFetch(query);
            Aver.AreEqual(1, affected);

            var c = NFX.DataAccess.MongoDB.Connector.MongoClient.Instance.DefaultLocalServer["nfxtest"]["MyPerzon"];
            var entries = c.FindAndFetchAll(new NFX.DataAccess.MongoDB.Connector.Query());
            Aver.AreEqual(3, entries.Count);

            var query1 = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id1) };
            var person1 = store.LoadRow(query1);
            Aver.IsNotNull(person1);
            Aver.AreEqual(id1, person1.GDID);
            Aver.AreEqual("Jack London", person1.Name);
            Aver.AreEqual(32, person1.Age);
            Aver.IsTrue(data.MemBufferEquals(person1.Data));

            var query2 = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id2) };
            var person2 = store.LoadRow(query2);
            Aver.IsNotNull(person2);
            Aver.AreEqual(id2, person2.GDID);
            Aver.AreEqual("Ivan Poddubny", person2.Name);
            Aver.AreEqual(41, person2.Age);

            var query3 = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id3) };
            var person3 = store.LoadRow(query3);
            Aver.IsNotNull(person3);
            Aver.AreEqual(id3, person3.GDID);
            Aver.AreEqual("Anna Smith", person3.Name);
            Aver.AreEqual(28, person3.Age);
        }

        [Run]
        public void ExecuteWithoutFetch_UpdateRows()
        {
            var id1 = new GDID(0, 0, 1);
            var id2 = new GDID(0, 0, 2);
            var id3 = new GDID(0, 0, 3);
            var data = new byte[] { 0x00, 0x79, 0x14 };
            var query = new Query<MyPerzon>("CRUD.InsertPerzons")
            {
                new Query.Param("id1", id1),
                new Query.Param("id2", id2),
                new Query.Param("id3", id3),
                new Query.Param("data", data)
            };
            store.ExecuteWithoutFetch(query);

            query = new Query<MyPerzon>("CRUD.UpdatePerzons")
            {
                new Query.Param("id1", id1),
                new Query.Param("id2", id2),
                new Query.Param("id3", id3),
            };
            var affected = store.ExecuteWithoutFetch(query);
            Aver.AreEqual(1, affected);

            query = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id1) };
            var person1 = store.LoadRow(query);
            Aver.IsNotNull(person1);
            Aver.AreEqual(id1, person1.GDID);
            Aver.AreEqual("Jack London", person1.Name);
            Aver.AreEqual(56, person1.Age);

            query = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id2) };
            var person2 = store.LoadRow(query);
            Aver.IsNotNull(person2);
            Aver.AreEqual(id2, person2.GDID);
            Aver.AreEqual("John", person2.Name);
            Aver.AreEqual(0, person2.Age); // update without $set removed Age field

            query = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id3) };
            var person3 = store.LoadRow(query);
            Aver.IsNotNull(person3);
            Aver.AreEqual(id3, person3.GDID);
            Aver.AreEqual("Anna Smith", person3.Name);
            Aver.AreEqual(23, person3.Age);
        }

        [Run]
        public void ExecuteWithoutFetch_Multiquering()
        {
            var id1 = new GDID(0, 0, 1);
            var id2 = new GDID(0, 0, 2);
            var id3 = new GDID(0, 0, 3);
            var data = new byte[] { 0x00, 0x79, 0x14 };
            var query1 = new Query<MyPerzon>("CRUD.InsertPerzons")
            {
                new Query.Param("id1", id1),
                new Query.Param("id2", id2),
                new Query.Param("id3", id3),
                new Query.Param("data", data)
            };
            var query2 = new Query<MyPerzon>("CRUD.UpdatePerzons")
            {
                new Query.Param("id1", id1),
                new Query.Param("id2", id2),
                new Query.Param("id3", id3)
            };

            var affected = store.ExecuteWithoutFetch(query1, query2);
            Aver.AreEqual(2, affected);

            var query = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id1) };
            var person1 = store.LoadRow(query);
            Aver.IsNotNull(person1);
            Aver.AreEqual(id1, person1.GDID);
            Aver.AreEqual("Jack London", person1.Name);
            Aver.AreEqual(56, person1.Age);

            query = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id2) };
            var person2 = store.LoadRow(query);
            Aver.IsNotNull(person2);
            Aver.AreEqual(id2, person2.GDID);
            Aver.AreEqual("John", person2.Name);
            Aver.AreEqual(0, person2.Age); // update without $set removed Age field

            query = new Query<MyPerzon>("CRUD.LoadPerzon") { new Query.Param("id", id3) };
            var person3 = store.LoadRow(query);
            Aver.IsNotNull(person3);
            Aver.AreEqual(id3, person3.GDID);
            Aver.AreEqual("Anna Smith", person3.Name);
            Aver.AreEqual(23, person3.Age);
        }

      [Run]
      public void SubDocuments_InsertAndLoad()
      {
          var row = new MyInvoice
          {
             GDID = new GDID(1, 1, 100),
             Name = "Dimon Khachaturyan",
             Date = new DateTime(1990, 08, 15, 14, 0, 0, DateTimeKind.Utc),
             Lines = new MyInvoiceLine[]
             {
              new MyInvoiceLine{ LineNo = 1, Description = "Sosiki", Amount = 12.67m },
              new MyInvoiceLine{ LineNo = 2, Description = "Mylo", Amount = 8.50m },
              new MyInvoiceLine{ LineNo = 3, Description = "Туалетная Бумага 'Зева'", Amount = 9.75m },
              new MyInvoiceLine{ LineNo = 4, Description = "Трусы Мужские", Amount = 3.72m }
             }
          };

          store.Insert(row);

          var row2 = store.LoadRow(new Query<MyInvoice>("CRUD.LoadInvoice")
                           {
                             new Query.Param("id", row.GDID)
                           });
          Aver.IsNotNull( row2 );

          Aver.AreEqual(row.GDID, row2.GDID);
          Aver.AreEqual(row.Name, row2.Name);
          Aver.AreEqual(row.Date, row2.Date);

          Aver.IsNotNull( row2.Lines );
          Aver.AreEqual(4, row2.Lines.Length );

          Aver.AreEqual(1, row2.Lines[0].LineNo );
          Aver.AreEqual("Sosiki", row2.Lines[0].Description );
          Aver.AreEqual(12.67m, row2.Lines[0].Amount );

          Aver.AreEqual(2, row2.Lines[1].LineNo );
          Aver.AreEqual("Mylo", row2.Lines[1].Description );
          Aver.AreEqual(8.50m, row2.Lines[1].Amount );

          Aver.AreEqual(3, row2.Lines[2].LineNo );
          Aver.AreEqual("Туалетная Бумага 'Зева'", row2.Lines[2].Description );
          Aver.AreEqual(9.75m, row2.Lines[2].Amount );

          Aver.AreEqual(4, row2.Lines[3].LineNo );
          Aver.AreEqual("Трусы Мужские", row2.Lines[3].Description );
          Aver.AreEqual(3.72m, row2.Lines[3].Amount );
      }


      [Run]
      public void SubDocuments_MuchData()
      {
          const int CNT = 1000;

          var row = new MuchData
          {
            Address1 = "1782 Zhabovaja Uliza # 2",
            Address2 = "Kv. # 18",
            AddressCity = "Odessa",
            AddressState = "Zhopinsk",
            AddressCountry = "Russia",

            Mother = new MyPerzon{ Name = "Alla Pugacheva", Age = 56, GDID = new GDID(1,12,456), Data = new byte[]{1,7,6,2,4}  },
            Father = new MyPerzon{ Name = "Tsoi Korolenko", Age = 52, GDID = new GDID(21,2,2456), Data = new byte[]{1,2,3,4,5} },

            Decimals = new decimal[]{ 23m, 234m, 12m, 90m, 234m},
            Double   = new double[] { 12.3d, 89.2d, 90d },
            Ints     = new int[]    { 23, 892,33,423 },
            Phone    = "22-3-22",

            Invoices = new MyInvoice[]
            {
                new MyInvoice
                {
                    GDID = new GDID(1, 1, 100),
                    Name = "Dimon Khachaturyan",
                    Date = new DateTime(1990, 08, 15, 14, 0, 0, DateTimeKind.Utc),
                    Lines = new MyInvoiceLine[]
                    {
                    new MyInvoiceLine{ LineNo = 1, Description = "Sosiki", Amount = 12.67m },
                    new MyInvoiceLine{ LineNo = 2, Description = "Mylo", Amount = 8.50m },
                    new MyInvoiceLine{ LineNo = 3, Description = "Туалетная Бумага 'Зева'", Amount = 9.75m },
                    new MyInvoiceLine{ LineNo = 4, Description = "Трусы Мужские", Amount = 3.72m }
                    }
                },

                new MyInvoice
                {
                    GDID = new GDID(1, 1, 200),
                    Name = "Karen Matnazarov",
                    Date = new DateTime(1990, 08, 15, 14, 0, 0, DateTimeKind.Utc),
                    Lines = new MyInvoiceLine[]
                    {
                    new MyInvoiceLine{ LineNo = 1, Description = "Sol", Amount = 2.67m },
                    new MyInvoiceLine{ LineNo = 2, Description = "Gazeta", Amount = 4.50m },
                    }
                }
            }
          };

          var sw = System.Diagnostics.Stopwatch.StartNew();

          row.GDID =  new GDID(10, 0);
          store.Insert(row);

          for(var i=1; i<CNT; i++)
          {
            row.GDID =  new GDID(10, (ulong)i);
            store.Insert(row);
          }

          var elp = sw.ElapsedMilliseconds;

          Console.WriteLine("Did {0} in {1} ms at {2} ops/sec".Args(CNT, elp, CNT / (elp / 1000d)));


          var row2 = store.LoadRow(new Query<MuchData>("CRUD.LoadMuchData")
                           {
                             new Query.Param("id", row.GDID)
                           });
          Aver.IsNotNull( row2 );

          Aver.AreEqual(row.Address1,    row2.Address1);
          Aver.AreEqual(row.Address2,    row2.Address2);
          Aver.AreEqual(row.AddressCity, row2.AddressCity);
          Aver.AreEqual(row.AddressState, row2.AddressState);
          Aver.AreEqual(row.AddressCountry, row2.AddressCountry);
          Aver.AreEqual(row.Invoices.Length, row2.Invoices.Length);
      }


      [Run]
      public void Key_Violation()
      {
        var data1 = new MyData{ ID = 1, Data = "My data string 1"};
        var data2 = new MyData{ ID = 2, Data = "My data string 2"};
        var data1again  = new MyData{ ID = 1, Data = "My data string 1 again"};

        store.Insert(data1);
        store.Insert(data2);

        try
        {
          store.Insert(data1again);
          Aver.Fail("No key violation");
        }
        catch(Exception error)
        {
          var dae = error as MongoDBDataAccessException;
          Aver.IsNotNull( dae );
          Aver.IsNotNull( dae.KeyViolation);
          Aver.IsTrue( dae.KeyViolationKind == NFX.DataAccess.KeyViolationKind.Primary);
          Console.WriteLine(error.ToMessageWithType());

          Console.WriteLine("Key violation is: "+dae.KeyViolation);
        }

        var rowset = store.LoadOneRowset(  new Query("CRUD.LoadAllMyData", typeof(MyData))  );
        Aver.IsNotNull( rowset );

        Aver.AreEqual(2, rowset.Count);

        Aver.AreObjectsEqual(1, rowset[0][0].AsInt());
        Aver.AreObjectsEqual(2, rowset[1][0].AsInt());
      }


        public class MyPerzon : TypedRow
        {
          [Field(backendName: "_id")]
          public GDID GDID { get; set;}

          [Field]
          public string Name { get; set;}

          [Field]
          public int Age { get; set;}

          [Field]
          public byte[] Data {get; set; }
        }

        public class MyData : TypedRow
        {
          [Field(backendName: "_id")]
          public long ID { get; set;}

          [Field]
          public string Data { get; set;}
        }



        public class MyInvoice : TypedRow
        {
          [Field(backendName: "_id")]
          public GDID GDID { get; set;}

          [Field(backendName: "name")]
          public string Name { get; set;}

          [Field(backendName: "dt")]
          public DateTime Date { get; set;}

          [Field(backendName: "lns")]
          public MyInvoiceLine[] Lines {get; set; }
        }

        public class MyInvoiceLine : TypedRow
        {
          [Field(backendName: "ln")]
          public int LineNo { get; set;}

          [Field(backendName: "d")]
          public string Description { get; set;}

          [Field(backendName: "a")]
          public decimal Amount { get; set;}
        }



        public class MuchData : TypedRow
        {
          [Field(backendName: "_id")]
          public GDID GDID { get; set;}

          [Field(backendName: "adr1")]
          public string Address1{ get; set;}

          [Field(backendName: "adr2")]
          public string Address2{ get; set;}

          [Field(backendName: "adrCity")]
          public string AddressCity{ get; set;}

          [Field(backendName: "adrState")]
          public string AddressState{ get; set;}

          [Field(backendName: "adrCountry")]
          public string AddressCountry{ get; set;}

          [Field(backendName: "phones")]
          public string Phone{ get; set;}

          [Field(backendName: "ints")]
          public int[] Ints{ get; set;}

          [Field(backendName: "doubles")]
          public double[] Double{ get; set;}

          [Field(backendName: "decimals")]
          public decimal[] Decimals{ get; set;}

          [Field(backendName: "m")]
          public MyPerzon Mother{ get; set;}

          [Field(backendName: "f")]
          public MyPerzon Father{ get; set;}

          [Field(backendName: "inv")]
          public MyInvoice[] Invoices{ get; set;}

          [Field(backendName: "i1")]
          public int Int1{ get; set; }

          [Field(backendName: "i2")]
          public int Int2{ get; set; }

          [Field(backendName: "i3")]
          public int Int3{ get; set; }

          [Field(backendName: "L1")]
          public long Long1{ get; set; }

          [Field(backendName: "L2")]
          public long Long2{ get; set; }

          [Field(backendName: "B1")]
          public bool Bool1{ get; set; }

          [Field(backendName: "B2")]
          public bool Bool2{ get; set; }
        }

  }
}
