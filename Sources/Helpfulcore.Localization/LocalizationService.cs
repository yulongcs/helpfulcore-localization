﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Publishing;
using Sitecore.SecurityModel;
using Sitecore.Web.UI.WebControls;

namespace Helpfulcore.Localization
{
	public class LocalizationService : ILocalizationService
	{
		private readonly LocalizationCache Cache = new LocalizationCache();

		private static readonly object CreateItemLock = new object();

		public virtual bool AutoCreateDictionaryItems { get; set; }
		public virtual bool UseDotSeparatedKeyNotaion { get; set; }

		public virtual string Localize(
			string key, 
			string defaultValue = null, 
			bool editable = false, 
			string language = null,
			bool autoCreate = true)
		{
			try
			{
				if (this.IsInEditingMode)
				{
					return this.Process(key, defaultValue, editable, language, autoCreate);
				}

				var value = this.Cache.Get(key);

				if (value == null)
				{
					value = this.Process(key, defaultValue, editable, language, autoCreate);
					this.Cache.Set(key, value);
				}

				return value;
			}
			catch
			{
				return Translate.Text(key);
			}
		}

		protected virtual string Process(string key, string defaultValue, bool editable, string language, bool autoCreate)
		{
			try
			{
				if (!this.AutoCreateDictionaryItems)
				{
					autoCreate = false;
				}

				if (string.IsNullOrWhiteSpace(key))
				{
					return defaultValue ?? key;
				}

				string localizedString;

				if (!autoCreate)
				{
					localizedString = this.TranslateText(key, language);

					if (!this.IsInEditingMode && !string.IsNullOrWhiteSpace(defaultValue) &&
					    localizedString.Equals(key, StringComparison.InvariantCultureIgnoreCase))
					{
						return defaultValue;
					}

					return localizedString;
				}

				if (this.IsInEditingMode && editable)
				{
					var item = this.GetDictionaryPhraseItem(key, language);
					if (item != null)
					{
						return new FieldRenderer {Item = item, FieldName = this.DictionaryPhraseFieldName}.Render();
					}
				}

				Translate.RemoveKeyFromCache(key);
				localizedString = this.TranslateText(key, language);

				if (localizedString.Equals(key, StringComparison.InvariantCultureIgnoreCase))
				{
					Translate.ResetCache(true);
					localizedString = this.TranslateText(key, language);
				}

				if (localizedString.Equals(key, StringComparison.InvariantCultureIgnoreCase))
				{
					localizedString = this.GetOrCreateDictionaryText(key, defaultValue, editable, language);
				}

				if (!editable || !this.IsInEditingMode)
				{
					localizedString = this.TranslateText(key, language);
				}

				if (!this.IsInEditingMode && !string.IsNullOrWhiteSpace(defaultValue) &&
				    (string.IsNullOrWhiteSpace(localizedString) ||
				     localizedString.Equals(key, StringComparison.InvariantCultureIgnoreCase)))
				{
					localizedString = defaultValue;
				}

				return localizedString;
			}
			catch (Exception ex)
			{
                Log.Error(string.Format("[LocalizationService]: Error while translating phrase with key '{0}'", key) + ex.Message, ex, this);

				return Translate.Text(key);
			}
		}

		protected virtual string TranslateText(string key, string language)
		{
			if (string.IsNullOrEmpty(language))
			{
				return Translate.Text(key);
			}

			var lang = LanguageManager.GetLanguage(language);
			if (lang != null)
			{
				return Translate.TextByLanguage(key, lang);
			}

			return Translate.Text(key);
		}

		protected virtual Item GetDictionaryPhraseItem(string key, string language)
		{
			var db = this.GetContentDatabase();

			if (db == null)
			{
				return null;
			}

			var dictionaryItemPath = this.GetDictionaryItemPath(key);

			if (!string.IsNullOrEmpty(language))
			{
				var lang = LanguageManager.GetLanguage(language);
				if (lang != null)
				{
					return db.GetItem(dictionaryItemPath, lang);
				}
			}

			return db.GetItem(dictionaryItemPath);
		}

		protected virtual string GetOrCreateDictionaryText(string key, string defaultValue, bool editable, string language)
		{
			var db = this.GetContentDatabase();

			if (db == null)
			{
				return key;
			}

			var item = this.GetDictionaryPhraseItem(key, language) ?? this.CreateDictionaryEntryItem(key, defaultValue, db, language);

			if (item != null)
			{
				if (string.IsNullOrWhiteSpace(item[this.DictionaryPhraseFieldName]) && !string.IsNullOrWhiteSpace(defaultValue))
				{
					this.UpdateDictionaryEntryItem(item, key, defaultValue, language);
				}

				return editable
					? new FieldRenderer {Item = item, FieldName = this.DictionaryPhraseFieldName}.Render()
					: string.IsNullOrWhiteSpace(item[this.DictionaryPhraseFieldName]) ? key : item[this.DictionaryPhraseFieldName];
			}

			return key;
		}

		protected virtual void UpdateDictionaryEntryItem(Item item, string key, string phraseValue, string language)
		{
			lock (CreateItemLock)
			{
				using (new SecurityDisabler())
				{
					item = this.SwhitchToLanguageVersion(language, item);

					try
					{
						item.Editing.BeginEdit();
						item[this.DictionaryPhraseFieldName] = phraseValue;
						Log.Debug(
							string.Format(
								"[LocalizationService]: Dictionary item with key '{0}' has been updated with default value '{1}'. Language: '{2}', dictionary item path: '{3}'", 
								key, 
								phraseValue, 
								item.Language.Name, 
								item.Paths.FullPath), 
							this);
					}
					catch
					{
						item.Editing.CancelEdit();
					}
					finally
					{
						item.Editing.EndEdit();
					}
				}

				this.PublishItem(item);

				Translate.RemoveKeyFromCache(key);
			}
		}

		protected virtual Item CreateDictionaryEntryItem(string key, string defaultValue, Database db, string language)
		{
			var dictionaryItemPath = this.GetDictionaryItemPath(key);

			lock (CreateItemLock)
			{
				Item item;
				using (new SecurityDisabler())
				{
					item = db.CreateItemPath(dictionaryItemPath, this.DictionaryTemplateFolder, this.DictionaryTemplateItem);

					item = this.SwhitchToLanguageVersion(language, item);

					try
					{
						item.Editing.BeginEdit();
						item[this.DictionaryKeyFieldName] = key;
						item[this.DictionaryPhraseFieldName] = defaultValue;

						Log.Debug(
							string.Format(
								"[LocalizationService]: Dictionary item with key '{0}' has been created with default value '{1}'. Language: '{2}', dictionary item path: '{3}'",
								key,
								defaultValue,
								item.Language.Name,
								item.Paths.FullPath),
							this);
					}
					catch
					{
						item.Editing.CancelEdit();
					}
					finally
					{
						item.Editing.EndEdit();
					}
				}

				this.PublishItem(item);

				Translate.RemoveKeyFromCache(key);

				return item;
			}
		}

		protected virtual Item SwhitchToLanguageVersion(string language, Item item)
		{
			if (!string.IsNullOrEmpty(language))
			{
				var lang = LanguageManager.GetLanguage(language);
				if (lang != null)
				{
					item = item.Database.GetItem(item.ID, lang);
					if (item.Versions.GetVersions(false).Length == 0)
					{
						item = item.Versions.AddVersion();
					}
				}
			}

			return item;
		}

		protected virtual Database GetContentDatabase()
		{
			if (Sitecore.Context.Site == null)
			{
				return null;
			}

			var db = Sitecore.Context.Site.ContentDatabase;
			if (db == null)
			{
				return null;
			}

			if (db.Name == "core")
			{
				db = Database.GetDatabase("master");
			}

			return !db.Name.Equals("master", StringComparison.InvariantCultureIgnoreCase) ? null : db;
		}

		protected Item GetDictionaryDomainItem()
		{
			var domainIdOrPath = "/sitecore/system/Dictionary";
			if (Sitecore.Context.Site != null && !string.IsNullOrWhiteSpace(Sitecore.Context.Site.DictionaryDomain))
			{
				domainIdOrPath = Sitecore.Context.Site.DictionaryDomain;
			}

			var db = this.GetContentDatabase();

			if (db != null)
			{
				var item = db.GetItem(domainIdOrPath);

				if (item != null)
				{
					return item;
				}
			}

			return null;
		}

		protected virtual string GetDictionaryItemPath(string key)
		{
			var relativeItemPath = key;

			if (this.UseDotSeparatedKeyNotaion)
			{
				var splittedValue = key.Split(new[] {"."}, StringSplitOptions.RemoveEmptyEntries);
				relativeItemPath = string.Join("/", splittedValue.Select(x => this.ToPascalCase(ItemUtil.ProposeValidItemName(x))));
			}

			var domainItem = this.GetDictionaryDomainItem();

			if (domainItem == null)
			{
				return "/sitecore/system/Dictionary/" + relativeItemPath;
			}

			return domainItem.Paths.FullPath + "/" + relativeItemPath;
		}

		protected virtual void PublishItem(Item item)
		{
			var toPublishList = new List<Item>();
			var dictionaryRoot = this.GetDictionaryDomainItem();
			if (dictionaryRoot != null)
			{
				var publishingItem = item;

				while (publishingItem.ID != dictionaryRoot.ID)
				{
					toPublishList.Add(publishingItem);
					publishingItem = publishingItem.Parent;
				}

				toPublishList.Reverse();

				foreach (var toPublish in toPublishList)
				{
					PublishManager.PublishItem(toPublish, this.PublishingTargets, new[] {toPublish.Language}, false, false);

					Log.Debug(
						string.Format(
							"[LocalizationService]: Dictionary item is being published... Language: '{0}', dictionary item path: '{1}'",
							item.Language.Name,
							item.Paths.FullPath),
						this);
				}
			}
			else
			{
				PublishManager.PublishItem(item, this.PublishingTargets, new[] {item.Language}, false, false);

				Log.Debug(
					string.Format(
						"[LocalizationService]: Dictionary item is being published... Language: '{0}', dictionary item path: '{1}'",
						item.Language.Name,
						item.Paths.FullPath),
					this);
			}
		}

		private Database[] publishingTargets;

		private Database[] PublishingTargets
		{
			get
			{
				if (this.publishingTargets == null)
				{
					var targets = new List<Database>();

					var publishingTargetsRoot = this.GetContentDatabase().GetItem("{D9E44555-02A6-407A-B4FC-96B9026CAADD}");
					foreach (Item target in publishingTargetsRoot.Children)
					{
						var value = target[ID.Parse("{39ECFD90-55D2-49D8-B513-99D15573DE41}")];
						if (target != null && !string.IsNullOrWhiteSpace(value))
						{
							var db = Database.GetDatabase(value);
							if (db != null)
							{
								targets.Add(db);
							}
						}
					}

					this.publishingTargets = targets.ToArray();
				}

				return this.publishingTargets;
			}
		}

		protected virtual string DictionaryKeyFieldName
		{
			get { return "Key"; }
		}

		protected virtual string DictionaryPhraseFieldName
		{
			get { return "Phrase"; }
		}

		protected virtual bool IsInEditingMode
		{
			get
			{
				return Sitecore.Context.PageMode.IsExperienceEditor|| Sitecore.Context.PageMode.IsExperienceEditorEditing;
			}
		}

		protected virtual TemplateItem DictionaryTemplateFolder
		{
			get { return this.GetContentDatabase().GetTemplate(new ID("{267D9AC7-5D85-4E9D-AF89-99AB296CC218}")); }
		}

		protected virtual TemplateItem DictionaryTemplateItem
		{
			get { return this.GetContentDatabase().GetTemplate(new ID("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}")); }
		}

		protected virtual string ToPascalCase(string s)
		{
			var result = new StringBuilder();
			var nonWordChars = new Regex(@"[^a-zA-Z0-9]+");
			var tokens = nonWordChars.Split(s);
			foreach (var token in tokens)
			{
				result.Append(this.PascalCaseSingleWord(token));
			}

			return result.ToString();
		}

		protected virtual string PascalCaseSingleWord(string s)
		{
			var match = Regex.Match(s, @"^(?<word>\d+|^[a-z]+|[A-Z]+|[A-Z][a-z]+|\d[a-z]+)+$");
			var groups = match.Groups["word"];

			var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
			var result = new StringBuilder();
			foreach (var capture in groups.Captures.Cast<Capture>())
			{
				result.Append(textInfo.ToTitleCase(capture.Value.ToLower()));
			}
			return result.ToString();
		}
	}
}
