using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Arena.Custom.HDC.GoogleMaps.UI
{
    public class DataGridWithHeaders : System.Web.UI.WebControls.DataGrid, INamingContainer
    {
        protected override void OnPreRender(EventArgs e)
        {
            Table table = Controls[0] as Table;

            if (table != null && table.Rows.Count > 0)
            {
                table.Rows[0].TableSection = TableRowSection.TableHeader;
                table.Rows[table.Rows.Count - 1].TableSection = TableRowSection.TableFooter;

                FieldInfo field = typeof(WebControl).GetField("tagKey", BindingFlags.Instance | BindingFlags.NonPublic);

                foreach (TableCell cell in table.Rows[0].Cells)
                {
                    field.SetValue(cell, HtmlTextWriterTag.Th);
                }
            }

            base.OnPreRender(e);
        }
    }
}
