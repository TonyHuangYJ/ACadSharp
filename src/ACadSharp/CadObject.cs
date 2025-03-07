﻿using ACadSharp.Attributes;
using ACadSharp.Objects;
using ACadSharp.Objects.Collections;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using ACadSharp.XData;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp
{
	/// <summary>
	/// Represents an element in a CadDocument.
	/// </summary>
	public abstract class CadObject : IHandledCadObject
	{
		/// <summary>
		/// Document where this element belongs.
		/// </summary>
		public CadDocument Document { get; private set; }

		/// <summary>
		/// Extended data attached to this object.
		/// </summary>
		public ExtendedDataDictionary ExtendedData { get; private set; }

		/// <inheritdoc/>
		/// <remarks>
		/// If the value is 0 the object is not assigned to a document or a parent.
		/// </remarks>
		[DxfCodeValue(5)]
		public ulong Handle { get; internal set; }

		/// <summary>
		/// Flag that indicates if this object has a dynamic dxf sublcass.
		/// </summary>
		public virtual bool HasDynamicSubclass { get { return false; } }

		/// <summary>
		/// The CAD class name of an object.
		/// </summary>
		public virtual string ObjectName { get; }

		/// <summary>
		/// Get the object type.
		/// </summary>
		public abstract ObjectType ObjectType { get; }

		/// <summary>
		/// Soft-pointer ID/handle to owner object.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 330)]
		public IHandledCadObject Owner { get; internal set; }

		/// <summary>
		/// Objects that are attached to this object.
		/// </summary>
		public IEnumerable<CadObject> Reactors { get { return this.reactors; } }

		/// <summary>
		/// Object Subclass marker.
		/// </summary>
		public abstract string SubclassMarker { get; }

		/// <summary>
		/// Extended Dictionary object.
		/// </summary>
		/// <remarks>
		/// An extended dictionary can be created using <see cref="CreateExtendedDictionary"/>.
		/// </remarks>
		public CadDictionary XDictionary
		{
			get { return this._xdictionary; }
			internal set
			{
				if (value == null)
					return;

				this._xdictionary = value;
				this._xdictionary.Owner = this;

				if (this.Document != null)
					this.Document.RegisterCollection(this._xdictionary);
			}
		}

		internal List<CadObject> reactors = new List<CadObject>();
		
		private CadDictionary _xdictionary = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public CadObject()
		{
			this.ExtendedData = new ExtendedDataDictionary(this);
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <remarks>
		/// The copy will be unattached from the document or any reference.
		/// </remarks>
		/// <returns>A new object that is a copy of this instance.</returns>
		public virtual CadObject Clone()
		{
			CadObject clone = (CadObject)this.MemberwiseClone();

			clone.Handle = 0;

			clone.Document = null;
			clone.Owner = null;

			//Collections
			clone.reactors = new List<CadObject>();
			clone.XDictionary = null;
			clone.ExtendedData = new ExtendedDataDictionary(clone);

			return clone;
		}

		/// <summary>
		/// Creates the extended dictionary if null.
		/// </summary>
		/// <returns>The <see cref="CadDictionary"/> attached to this <see cref="CadObject"/></returns>
		public CadDictionary CreateExtendedDictionary()
		{
			if (this._xdictionary == null)
			{
				this.XDictionary = new CadDictionary();
			}

			return this._xdictionary;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.ObjectName}:{this.Handle}";
		}

		internal virtual void AssignDocument(CadDocument doc)
		{
			this.Document = doc;

			if (this.XDictionary != null)
			{
				doc.RegisterCollection(this.XDictionary);
			}

			if (this.ExtendedData.Any())
			{
				//Reset existing collection
				var entries = this.ExtendedData.ToArray();
				this.ExtendedData.Clear();

				foreach (var item in entries)
				{
					this.ExtendedData.Add(item.Key, item.Value);
				}
			}
		}

		internal virtual void UnassignDocument()
		{
			if (this.XDictionary != null)
				this.Document.UnregisterCollection(this.XDictionary);

			this.Handle = 0;
			this.Document = null;

			if (this.ExtendedData.Any())
			{
				//Reset existing collection
				var entries = this.ExtendedData.ToArray();
				this.ExtendedData.Clear();

				foreach (var item in entries)
				{
					this.ExtendedData.Add(item.Key.Clone() as AppId, item.Value);
				}
			}
		}

		protected T updateCollection<T>(T entry, ObjectDictionaryCollection<T> collection)
			where T : NonGraphicalObject
		{
			if (collection == null || entry == null)
			{
				return entry;
			}

			if (collection.TryGetValue(entry.Name, out T existing))
			{
				return existing;
			}
			else
			{
				collection.Add(entry);
				return entry;
			}
		}

		protected T updateTable<T>(T entry, Table<T> table)
					where T : TableEntry
		{
			if (table == null)
			{
				return entry;
			}

			if (table.TryGetValue(entry.Name, out T existing))
			{
				return existing;
			}
			else
			{
				table.Add(entry);
				return entry;
			}
		}
	}
}