using System;
using System.Collections.Generic;
using System.Linq;
using SmartAddresser.Editor.Core.Models.LayoutRules.LabelRules;
using SmartAddresser.Editor.Foundation.EasyTreeView;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SmartAddresser.Editor.Core.Tools.Addresser.LayoutRuleEditor.LabelRuleEditor
{
    /// <summary>
    ///     Tree view for the Label Rule Editor.
    /// </summary>
    internal sealed class LabelRuleListTreeView : TreeViewBase
    {
        public enum Columns
        {
            Name,
            AssetGroups,
            LabelRule
        }

        [NonSerialized] private int _currentId;

        public LabelRuleListTreeView(State state) : base(state)
        {
            showAlternatingRowBackgrounds = true;
            ColumnStates = state.ColumnStates;
            rowHeight = 16;
            Reload();
        }

        public Item AddItem(LabelRule rule, int index = -1)
        {
            rule.RefreshLabelProviderDescription();
            rule.RefreshAssetGroupDescription();
            var item = new Item(rule)
            {
                id = _currentId++
            };
            item.displayName = rule.Name.Value;
            AddItemAndSetParent(item, -1, index);
            return item;
        }

        protected override void CellGUI(int columnIndex, Rect cellRect, RowGUIArgs args)
        {
            var item = (Item)args.item;
            switch ((Columns)columnIndex)
            {
                case Columns.Name:
                    args.rowRect = cellRect;
                    base.CellGUI(columnIndex, cellRect, args);
                    break;
                case Columns.AssetGroups:
                    GUI.Label(cellRect, GetText(item, columnIndex));
                    break;
                case Columns.LabelRule:
                    GUI.Label(cellRect, GetText(item, columnIndex));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override IOrderedEnumerable<TreeViewItem> OrderItems(IList<TreeViewItem> items, int keyColumnIndex,
            bool ascending)
        {
            string KeySelector(TreeViewItem x)
            {
                return GetText((Item)x, keyColumnIndex);
            }

            return ascending
                ? items.OrderBy(KeySelector, Comparer<string>.Create(EditorUtility.NaturalCompare))
                : items.OrderByDescending(KeySelector, Comparer<string>.Create(EditorUtility.NaturalCompare));
        }

        protected override string GetTextForSearch(TreeViewItem item, int columnIndex)
        {
            return GetText((Item)item, columnIndex);
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return true;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (args.acceptedRename)
            {
                var item = (Item)GetItem(args.itemID);
                item.Rule.Name.Value = args.newName;
                item.displayName = args.newName;
                Reload();
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return false;
        }

        private string GetText(Item item, int columnIndex)
        {
            switch ((Columns)columnIndex)
            {
                case Columns.Name:
                    return item.Rule.Name.Value;
                case Columns.AssetGroups:
                    if (GetSelection().FirstOrDefault() == item.id)
                        item.Rule.RefreshAssetGroupDescription();
                    return item.Rule.AssetGroupDescription.Value;
                case Columns.LabelRule:
                    if (GetSelection().FirstOrDefault() == item.id)
                        item.Rule.RefreshLabelProviderDescription();
                    return item.Rule.LabelProviderDescription.Value;
                default:
                    throw new NotImplementedException();
            }
        }

        public sealed class Item : TreeViewItem
        {
            public Item(LabelRule rule)
            {
                Rule = rule;
            }

            public LabelRule Rule { get; }
        }

        [Serializable]
        public sealed class State : TreeViewState
        {
            [SerializeField] private MultiColumnHeaderState.Column[] _columnStates;

            public State()
            {
                _columnStates = GetColumnStates();
            }

            public MultiColumnHeaderState.Column[] ColumnStates => _columnStates;

            private MultiColumnHeaderState.Column[] GetColumnStates()
            {
                var nameColumn = new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Center,
                    canSort = false,
                    width = 150,
                    minWidth = 50,
                    autoResize = false,
                    allowToggleVisibility = false
                };
                var assetGroupsColumn = new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Asset Groups"),
                    headerTextAlignment = TextAlignment.Center,
                    canSort = false,
                    width = 200,
                    minWidth = 50,
                    autoResize = true,
                    allowToggleVisibility = true
                };
                var labelRuleColumn = new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Label Rule"),
                    headerTextAlignment = TextAlignment.Center,
                    canSort = false,
                    width = 200,
                    minWidth = 50,
                    autoResize = true,
                    allowToggleVisibility = true
                };
                return new[] { nameColumn, assetGroupsColumn, labelRuleColumn };
            }
        }
    }
}