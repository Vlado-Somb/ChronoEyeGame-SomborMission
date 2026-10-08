using System.Collections;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace UITKUtils
{
    [UxmlElement]
    public partial class Repeater : VisualElement
    {
        [UxmlAttribute]
        public VisualTreeAsset asset;

        int m_ItemCount;

        public Repeater()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttached);
            RegisterCallback<DetachFromPanelEvent>(OnRemoved);
        }

        void OnRemoved(DetachFromPanelEvent evt)
        {
            m_ItemCount = 0;
            Clear();
        }

        void CheckCount(TimerState obj)
        {
            // TODO evaluate if we can avoid this every update and bind to a private IList local property instead
            // to detect any data source / path change etc.
            DataSourceContext context = GetHierarchicalDataSourceContext();
            
            if (context.dataSource != null && !context.dataSourcePath.IsEmpty)
            {
                var list = PropertyContainer.GetValue<object, IList>(context.dataSource, context.dataSourcePath);

                if (asset == null || list == null)
                {
                    Clear();
                    return;
                }

                m_ItemCount = list.Count;
                
                for (int i = 0; i < m_ItemCount; i++)
                {
                    if (i >= childCount)
                    {
                        // TODO provide the option to have or not have the root item (i.e. use Instantiate())
                        /*var tree = asset.CloneTree();
                        Add(tree);   */
                        asset.CloneTree(this);
                    }
                    this[i].dataSource = context.dataSource;
                    this[i].dataSourcePath = PropertyPath.AppendIndex(context.dataSourcePath, i);
                }

                while (childCount > m_ItemCount)
                {
                    RemoveAt(childCount-1);
                }
            }
            else
            {
                Clear();
            }
        }

        void OnAttached(AttachToPanelEvent evt)
        {
            schedule.Execute(CheckCount).Every(1000 / Application.targetFrameRate);
            
            DataSourceContext context = GetHierarchicalDataSourceContext();

            if (context.dataSource != null && !context.dataSourcePath.IsEmpty)
            {
                var list = PropertyContainer.GetValue<object, IList>(context.dataSource, context.dataSourcePath);

                if (asset == null || list == null)
                    return;

                m_ItemCount = list.Count;
                
                Clear();
                
                for (int i = 0; i < m_ItemCount; i++)
                {
                    // TODO provide the option to have or not have the root item (i.e. use Instantiate())
                    asset.CloneTree(this);
                    // could do some type checking here
                    this[i].dataSource = context.dataSource;
                    this[i].dataSourcePath = PropertyPath.AppendIndex(context.dataSourcePath, i);
                }
            }
        }
    }
}