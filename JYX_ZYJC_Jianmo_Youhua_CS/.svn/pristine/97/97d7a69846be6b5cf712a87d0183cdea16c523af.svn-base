using System.Collections.Generic;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class CellElementEditor
    {
        public static void SetCellChildClassTypeToCon() {
            Bentley.DgnPlatformNET.ModelElementsCollection elements = Bentley.MstnPlatformNET.Session.Instance.GetActiveDgnModel().GetGraphicElements();//扫描所有元素
            IEnumerator<Bentley.DgnPlatformNET.Elements.Element> enumerator1 = elements.GetEnumerator();
            List<Bentley.DgnPlatformNET.Elements.Element> gasketElement = new List<Bentley.DgnPlatformNET.Elements.Element>();

            while (enumerator1.MoveNext())//拿到所有的 Gasket
            {
                if (enumerator1.Current.Description.Contains("Gasket"))
                {
                    gasketElement.Add(enumerator1.Current);
                }
            }
            //TODO 遍历拿到的 Gasket 对象，重新构建形体的Cell
            foreach (Bentley.DgnPlatformNET.Elements.Element item in gasketElement)
            {
                Bentley.DgnPlatformNET.Elements.ChildElementCollection childCollection = item.GetChildren();
                List<Bentley.DgnPlatformNET.Elements.Element> dependantsElement = item.GetDependants();
                ushort[] linkageIds = item.GetLinkageIds();

                List<Bentley.DgnPlatformNET.Elements.Element> tempList = new List<Bentley.DgnPlatformNET.Elements.Element>();
                Bentley.DgnPlatformNET.Elements.ChildElementCollection children2 = item.GetChildren();
                IEnumerator<Bentley.DgnPlatformNET.Elements.Element> enumerator3 = children2.GetEnumerator();
                while (enumerator3.MoveNext())
                {
                    Bentley.DgnPlatformNET.Elements.CellHeaderElement currentCellElement = enumerator3.Current as Bentley.DgnPlatformNET.Elements.CellHeaderElement;
                    if (currentCellElement.CellName.Equals("Graphics"))
                    {
                        IEnumerator<Bentley.DgnPlatformNET.Elements.Element> enumerator4 = enumerator3.Current.GetChildren().GetEnumerator();
                        while (enumerator4.MoveNext())
                        {
                            if (enumerator4.Current.ElementType == Bentley.DgnPlatformNET.MSElementType.LineString)
                            {
                                //TODO 修改该单元
                                //Bentley.DgnPlatformNET.ElementCopyContext copyContext = new Bentley.DgnPlatformNET.ElementCopyContext(Bentley.MstnPlatformNET.Session.Instance.GetActiveDgnModelRef());
                                try
                                {
                                    //Bentley.DgnPlatformNET.Elements.Element tempElement = copyContext.DoCopy(enumerator4.Current);
                                    //Bentley.DgnPlatformNET.Elements.Element newElement = enumerator4.Current;
                                    //Bentley.DgnPlatformNET.ElementPropertiesSetter prop_setter = new Bentley.DgnPlatformNET.ElementPropertiesSetter();
                                    //prop_setter.SetElementClass(Bentley.DgnPlatformNET.DgnElementClass.Construction);
                                    //prop_setter.Apply(enumerator4.Current);
                                    //enumerator4.Current.ReplaceInModel(enumerator4.Current);
                                    Bentley.Interop.MicroStationDGN.Element v8iElement = JYX_ZYJC_CLR.PublicMethod.convertToInteropElem(enumerator4.Current);
                                    //Bentley.Interop.MicroStationDGN.CellElement v8iCellELement = v8iElement.AsCellElement();
                                    v8iElement.Class = Bentley.Interop.MicroStationDGN.MsdElementClass.Construction;
                                    v8iElement.Rewrite();
                                    //tempElement.AddToModel();
                                }
                                catch (System.Exception exc)
                                {
                                    System.Windows.Forms.MessageBox.Show(exc.ToString());
                                }
                            }
                            else
                            {
                                //TODO 复制该单元
                                //System.Windows.Forms.MessageBox.Show(enumerator4.Current.ElementType.ToString());
                            }
                        }
                    }
                    else
                    {
                        //TODO 复制该单元
                        //Bentley.DgnPlatformNET.Elements.Element tempelement = ;
                    }
                }
            }

        }
    }
}
