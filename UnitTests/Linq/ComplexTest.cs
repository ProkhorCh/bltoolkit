﻿using System;
using System.Linq;

using NUnit.Framework;

using BLToolkit.Data.DataProvider;

namespace Data.Linq
{
	[TestFixture]
	public class ComplexTest : TestBase
	{
		[Test]
		public void Contains1()
		{
			var q1 =
				from gc in GrandChild
					join max in
						from gch in GrandChild
						group gch by gch.ChildID into g
						select g.Max(c => c.GrandChildID)
					on gc.GrandChildID equals max
				select gc;

			var expected =
				from ch in Child
					join p  in Parent on ch.ParentID equals p.ParentID
					join gc in q1     on p.ParentID  equals gc.ParentID into g
					from gc in g.DefaultIfEmpty()
				where gc == null || !new[] { 111, 222 }.Contains(gc.GrandChildID.Value)
				select new { p.ParentID, gc };

			ForEachProvider(new [] { ProviderName.Firebird, ProviderName.Access }, db =>
			{
				var q2 =
					from gc in db.GrandChild
						join max in
							from gch in db.GrandChild
							group gch by gch.ChildID into g
							select g.Max(c => c.GrandChildID)
						on gc.GrandChildID equals max
					select gc;

				var result =
					from ch in db.Child
						join p  in db.Parent on ch.ParentID equals p.ParentID
						join gc in q2     on p.ParentID  equals gc.ParentID into g
						from gc in g.DefaultIfEmpty()
				where gc == null || !new[] { 111, 222 }.Contains(gc.GrandChildID.Value)
				select new { p.ParentID, gc };

				AreEqual(expected, result);
			});
		}

		[Test]
		public void Contains2()
		{
			var q1 =
				from gc in GrandChild
					join max in
						from gch in GrandChild
						group gch by gch.ChildID into g
						select g.Max(c => c.GrandChildID)
					on gc.GrandChildID equals max
				select gc;

			var expected =
				from ch in Child
					join p  in Parent on ch.ParentID equals p.ParentID
					join gc in q1     on p.ParentID  equals gc.ParentID into g
					from gc in g.DefaultIfEmpty()
				where gc == null || gc.GrandChildID != 111 && gc.GrandChildID != 222
				select new
				{
					Parent       = p,
					GrandChildID = gc,
					Value        = GetValue(gc != null ? gc.ChildID : int.MaxValue)
				};

			ForEachProvider(new [] { ProviderName.Firebird, ProviderName.Access }, db =>
			{
				var q2 =
					from gc in db.GrandChild
						join max in
							from gch in db.GrandChild
							group gch by gch.ChildID into g
							select g.Max(c => c.GrandChildID)
						on gc.GrandChildID equals max
					select gc;

				var result =
					from ch in db.Child
						join p  in db.Parent on ch.ParentID equals p.ParentID
						join gc in q2     on p.ParentID  equals gc.ParentID into g
						from gc in g.DefaultIfEmpty()
				where gc == null || gc.GrandChildID != 111 && gc.GrandChildID != 222
				select new
				{
					Parent       = p,
					GrandChildID = gc,
					Value        = GetValue(gc != null ? gc.ChildID : int.MaxValue)
				};

				AreEqual(expected, result);
			});
		}

		static int GetValue(int? value)
		{
			return value ?? 777;
		}

		[Test]
		public void Contains3()
		{
			var q1 =
				from gc in GrandChild1
					join max in
						from gch in GrandChild1
						group gch by gch.ChildID into g
						select g.Max(c => c.GrandChildID)
					on gc.GrandChildID equals max
				select gc;

			var expected =
				from ch in Child
					join p  in Parent on ch.ParentID equals p.ParentID
					join gc in q1     on p.ParentID  equals gc.ParentID into g
					from gc in g.DefaultIfEmpty()
				where gc == null || !new[] { 111, 222 }.Contains(gc.GrandChildID.Value)
				select new { p.ParentID, gc };

			ForEachProvider(new [] { ProviderName.SQLite, ProviderName.Access }, db =>
			{
				var q2 =
					from gc in db.GrandChild1
						join max in
							from gch in db.GrandChild1
							group gch by gch.ChildID into g
							select g.Max(c => c.GrandChildID)
						on gc.GrandChildID equals max
					select gc;

				var result =
					from ch in db.Child
						join p  in db.Parent on ch.ParentID equals p.ParentID
						join gc in q2     on p.ParentID  equals gc.ParentID into g
						from gc in g.DefaultIfEmpty()
				where gc == null || !new[] { 111, 222 }.Contains(gc.GrandChildID.Value)
				select new { p.ParentID, gc };

				AreEqual(expected, result);
			});
		}

		[Test]
		public void Contains4()
		{
			var q1 =
				from gc in GrandChild1
					join max in
						from gch in GrandChild1
						group gch by gch.ChildID into g
						select g.Max(c => c.GrandChildID)
					on gc.GrandChildID equals max
				select gc;

			var expected =
				from ch in Child
					join gc in q1 on ch.Parent.ParentID equals gc.ParentID into g
					from gc in g.DefaultIfEmpty()
				where gc == null || !new[] { 111, 222 }.Contains(gc.GrandChildID.Value)
				select new { ch.Parent, gc };

			ForEachProvider(new [] { ProviderName.SQLite, ProviderName.Access }, db =>
			{
				var q2 =
					from gc in db.GrandChild1
						join max in
							from gch in db.GrandChild1
							group gch by gch.ChildID into g
							select g.Max(c => c.GrandChildID)
						on gc.GrandChildID equals max
					select gc;

				var result =
					from ch in db.Child
						join gc in q2 on ch.Parent.ParentID equals gc.ParentID into g
						from gc in g.DefaultIfEmpty()
				where gc == null || !new[] { 111, 222 }.Contains(gc.GrandChildID.Value)
				select new { ch.Parent, gc };

				AreEqual(expected, result);
			});
		}

		[Test]
		public void Join1()
		{
			var q1 =
				from p in Parent
					join c in Child      on p.ParentID equals c.ParentID
					join g in GrandChild on p.ParentID equals g.ParentID
				select new { p, c, g };

			var expected =
				from x in q1
				where
				(
					(x.c.ParentID == 2 || x.c.ParentID == 3) && x.g.ChildID != 21 && x.g.ChildID != 33
				) || (
					x.g.ParentID == 3 && x.g.ChildID == 32
				) || (
					x.g.ChildID == 11
				)
				select x;

			ForEachProvider(db =>
			{
				var q2 =
					from p in db.Parent
						join c in db.Child      on p.ParentID equals c.ParentID
						join g in db.GrandChild on p.ParentID equals g.ParentID
					select new { p, c, g };

				var result =
					from x in q2
					where
					(
						(x.c.ParentID == 2 || x.c.ParentID == 3) && x.g.ChildID != 21 && x.g.ChildID != 33
					) || (
						x.g.ParentID == 3 && x.g.ChildID == 32
					) || (
						x.g.ChildID == 11
					)
					select x;

					AreEqual(expected, result);
			});
		}
	}
}