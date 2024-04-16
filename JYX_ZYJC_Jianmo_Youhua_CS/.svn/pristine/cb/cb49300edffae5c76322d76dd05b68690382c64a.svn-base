using Bentley.DgnPlatformNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.DgnPlatformNET.Elements;
using System.Windows.Forms;
using Bentley.GeometryNET;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class ToolsTemplate : DgnElementSetTool
    {
        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Success;
        }

        protected override void OnRestartTool()
        {
        }

        protected override bool AcceptIdentifiesNext()
        {
            return base.AcceptIdentifiesNext();
        }
        protected override UsesDragSelect AllowDragSelect()
        {
            //return UsesDragSelect.Line;
            return UsesDragSelect.Line;
        }
        /// <summary>
        /// 3
        /// </summary>
        /// <returns></returns>
        protected override UsesFence AllowFence()
        {
            return base.AllowFence();
        }
        /// <summary>
        /// 4
        /// </summary>
        /// <returns></returns>
        protected override UsesSelection AllowSelection()
        {
            return base.AllowSelection();
        }
        protected override void BeginDynamics()
        {
            base.BeginDynamics();
        }
        protected override void BeginPickElements()
        {
            base.BeginPickElements();
        }
        protected override void BuildAgenda(DgnButtonEvent ev)
        {
            base.BuildAgenda(ev);
        }
        protected override bool BuildDragSelectAgenda(FenceParameters fp, DgnButtonEvent ev)
        {
            return base.BuildDragSelectAgenda(fp, ev);
        }
        protected override Element BuildLocateAgenda(HitPath path, DgnButtonEvent ev)
        {
            return base.BuildLocateAgenda(path, ev);
        }
        protected override bool CheckSingleShot()
        {
            return base.CheckSingleShot();
        }
        public override bool CheckStop()
        {
            return base.CheckStop();
        }
        public override void ClearCopyContext()
        {
            base.ClearCopyContext();
        }
        protected override void DecorateScreen(Viewport vp)
        {
            base.DecorateScreen(vp);
        }
        protected override bool DisableEditAction()
        {
            return base.DisableEditAction();
        }
        protected override void Dispose(bool A_0)
        {
            base.Dispose(A_0);
        }
        protected override StatusInt DoFenceClip()
        {
            return base.DoFenceClip();
        }
        protected override bool DoGroups()
        {
            return base.DoGroups();
        }
        protected override HitPath DoLocate(DgnButtonEvent ev, bool newSearch, int complexComponent)
        {
            return base.DoLocate(ev, newSearch, complexComponent);
        }
        public override StatusInt DoOperationForModify(Element element)
        {
            return base.DoOperationForModify(element);
        }
        protected override void EndDynamics()
        {
            base.EndDynamics();
        }
        protected override void ExitTool()
        {
            base.ExitTool();
        }
        //TODO
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        protected override bool FilterAgendaEntries()
        {
            return base.FilterAgendaEntries();
        }
        protected override int GetAdditionalLocateNumRequired()
        {
            return base.GetAdditionalLocateNumRequired();
        }
        protected override AgendaModify GetAgendaModify()
        {
            return base.GetAgendaModify();
        }
        protected override AgendaOperation GetAgendaOperation()
        {
            return base.GetAgendaOperation();
        }
        protected override bool GetAnchorPoint(out DPoint3d anchorPt)
        {
            return base.GetAnchorPoint(out anchorPt);
        }
        protected override DPoint3d[] GetBoxPoints(DgnCoordSystem sys, DPoint3d activeOrigin, DPoint3d activeCorner, Viewport vp)
        {
            return base.GetBoxPoints(sys, activeOrigin, activeCorner, vp);
        }
        /// <summary>
        /// 7
        /// </summary>
        /// <returns></returns>
        protected override DgnModelRef GetDestinationModelRef()
        {
            return base.GetDestinationModelRef();
        }
        protected override bool GetDragAnchorPoint(out DPoint3d anchorPt)
        {
            return base.GetDragAnchorPoint(out anchorPt);
        }
        protected override bool GetDragSelectOverlapMode(DgnButtonEvent ev)
        {
            return base.GetDragSelectOverlapMode(ev);
        }
        protected override void GetDragSelectSymbology(out uint color, out uint fillColor, out uint style, out uint weight, DgnButtonEvent ev)
        {
            base.GetDragSelectSymbology(out color, out fillColor, out style, out weight, ev);
        }
        protected override IDrawElementAgenda GetDrawDynamicsTxnChanges()
        {
            return base.GetDrawDynamicsTxnChanges();
        }
        /// <summary>
        /// 6
        /// </summary>
        /// <returns></returns>
        protected override ElementSource GetElementSource()
        {
            return base.GetElementSource();
        }
        protected override ClipResult GetFenceClipResult()
        {
            return base.GetFenceClipResult();
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// 2
        /// </summary>
        /// <returns></returns>
        protected override ElementSource GetPreferredElementSource()
        {
            return base.GetPreferredElementSource();//里面调用了3,4
        }
        /// <summary>
        /// 8
        /// </summary>
        /// <returns></returns>
        protected override RefLocateOption GetReferenceLocateOptions()
        {
            return base.GetReferenceLocateOptions();
        }
        /// <summary>
        /// 9
        /// </summary>
        /// <returns></returns>
        protected override string GetToolName()
        {
            return base.GetToolName();
        }
        protected override void HiliteAgendaEntries(bool changed)
        {
            base.HiliteAgendaEntries(changed);
        }
        protected override bool HiliteFenceElems()
        {
            return base.HiliteFenceElems();
        }
        protected override UsesDragSelect IsDragSelectActive()
        {
            return base.IsDragSelectActive();
        }
        protected override bool IsFenceClip()
        {
            return base.IsFenceClip();
        }
        protected override bool IsFenceOverlap()
        {
            return base.IsFenceOverlap();
        }
        protected override bool IsFenceVoid()
        {
            return base.IsFenceVoid();
        }
        protected override bool IsModifyOriginal()
        {
            return base.IsModifyOriginal();
        }
        protected override bool IsSingleShot()
        {
            return base.IsSingleShot();
        }
        protected override void LocateOneElement(DgnButtonEvent ev, bool newSearch)
        {
            base.LocateOneElement(ev, newSearch);
        }
        protected override void ModifyAgendaEntries()
        {
            base.ModifyAgendaEntries();
        }
        protected override bool NeedAcceptPoint()
        {
            return base.NeedAcceptPoint();
            //return false;
        }
        protected override bool NeedPointForDynamics()
        {
            return base.NeedPointForDynamics();
        }
        protected override bool NeedPointForSelection()
        {
            return base.NeedPointForSelection();
        }
        protected override bool On3DInputEvent(Dgn3DInputEvent ev)
        {
            return base.On3DInputEvent(ev);
        }
        protected override void OnCleanup()
        {
            base.OnCleanup();
        }
        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            return base.OnDataButton(ev);
            //return true;
        }
        protected override bool OnDataButtonUp(DgnButtonEvent ev)
        {
            return base.OnDataButtonUp(ev);
        }
        protected override void OnDynamicFrame(DgnButtonEvent ev)
        {
            base.OnDynamicFrame(ev);
        }
        protected override StatusInt OnElementModifyClip(Element el, FenceParameters fp, FenceClipFlags options)
        {
            return base.OnElementModifyClip(el, fp, options);
        }
        protected override bool OnFlick(DgnFlickEvent ev)
        {
            return base.OnFlick(ev);
        }
        protected override bool OnGesture(DgnGestureEvent ev)
        {
            return base.OnGesture(ev);
        }
        protected override bool OnGestureNotify(IndexedViewport A_0, long A_1)
        {
            return base.OnGestureNotify(A_0, A_1);
        }
        /// <summary>
        /// 6
        /// </summary>
        /// <returns></returns>
        protected override bool OnInstall()
        {
            return base.OnInstall();
        }
        protected override bool OnModelEndDrag(DgnButtonEvent ev)
        {
            return base.OnModelEndDrag(ev);
        }
        protected override bool OnModelMotion(DgnButtonEvent ev)
        {
            return base.OnModelMotion(ev);
        }
        protected override bool OnModelMotionStopped(DgnButtonEvent ev)
        {
            return base.OnModelMotionStopped(ev);
        }
        protected override bool OnModelNoMotion(DgnButtonEvent ev)
        {
            return base.OnModelNoMotion(ev);
        }
        protected override bool OnModelStartDrag(DgnButtonEvent ev)
        {
            return base.OnModelStartDrag(ev);
        }
        protected override bool OnModifierKeyTransition(bool wentDown, int key)
        {
            return base.OnModifierKeyTransition(wentDown, key);
        }
        protected override bool OnModifyComplete(DgnButtonEvent ev)
        {
            return base.OnModifyComplete(ev);
        }
        protected override bool OnMouseWheel(DgnMouseWheelEvent ev)
        {
            return base.OnMouseWheel(ev);
        }
        /// <summary>
        /// 10
        /// </summary>
        protected override void OnPostInstall()
        {
            base.OnPostInstall();
        }
        protected override bool OnPostLocate(HitPath path, out string cantAcceptReason)
        {
            return base.OnPostLocate(path, out cantAcceptReason);
        }
        public override StatusInt OnPreElementModify(Element element)
        {
            return base.OnPreElementModify(element);
        }
        protected override bool OnPreFilterButtonEvent(Viewport __unnamed000, out bool testDefault)
        {
            return base.OnPreFilterButtonEvent(__unnamed000, out testDefault);
        }
        public override StatusInt OnRedrawComplete(ViewContext context)
        {
            return base.OnRedrawComplete(context);
        }
        public override void OnRedrawInit(ViewContext context)
        {
            base.OnRedrawInit(context);
        }
        public override StatusInt OnRedrawOperation(Element el, ViewContext context, out bool canUseCached)
        {
            return base.OnRedrawOperation(el, context, out canUseCached);
        }
        protected override void OnReinitialize()
        {
            base.OnReinitialize();
        }
        protected override bool OnResetButton(DgnButtonEvent ev)
        {
            return base.OnResetButton(ev);
        }
        protected override bool OnResetButtonUp(DgnButtonEvent ev)
        {
            return base.OnResetButtonUp(ev);
        }
        public override void OnResymbolize(ViewContext context)
        {
            base.OnResymbolize(context);
        }
        protected override int OnTabletQuerySystemGestureStatus(DgnButtonEvent ev)
        {
            return base.OnTabletQuerySystemGestureStatus(ev);
        }
        protected override bool OnTouch(DgnTouchEvent ev)
        {
            return base.OnTouch(ev);
        }
        protected override void OnUndoPreviousStep()
        {
            base.OnUndoPreviousStep();
        }
        protected override StatusInt PerformEditAction(int index)
        {
            return base.PerformEditAction(index);
        }
        protected override bool PopulateToolSettings()
        {
            return base.PopulateToolSettings();
        }
        protected override StatusInt ProcessAgenda(DgnButtonEvent ev)
        {
            return base.ProcessAgenda(ev);
        }
        protected override void RemoveAgendaElement(Element el)
        {
            base.RemoveAgendaElement(el);
        }
        public override void ResetStop()
        {
            base.ResetStop();
        }
        protected override void SetAnchorPoint(DPoint3d anchorPt)
        {
            base.SetAnchorPoint(anchorPt);
        }
        /// <summary>
        /// 5
        /// </summary>
        /// <param name="source"></param>
        protected override void SetElementSource(ElementSource source)
        {
            base.SetElementSource(source);
        }
        protected override void SetLocateCriteria()
        {
            base.SetLocateCriteria();
        }
        protected override void SetLocateCursor(bool enableLocate)
        {
            base.SetLocateCursor(enableLocate);
        }
        /// <summary>
        /// 11
        /// </summary>
        protected override void SetupAndPromptForNextAction()
        {
            base.SetupAndPromptForNextAction();
        }
        protected override bool SetupForModify(DgnButtonEvent ev, bool isDynamics)
        {
            return base.SetupForModify(ev, isDynamics);
        }
        public override void SetWantGraphicGroupLock(bool value)
        {
            base.SetWantGraphicGroupLock(value);
        }
        public override void SetWantMakeCopy(bool value)
        {
            base.SetWantMakeCopy(value);
        }
        public override string ToString()
        {
            return base.ToString();
        }
        protected override void UnHiliteAgendaEntries(bool empty)
        {
            base.UnHiliteAgendaEntries(empty);
        }
        protected override bool UseActiveFence()
        {
            return base.UseActiveFence();
        }
        /// <summary>
        /// 12
        /// </summary>
        /// <returns></returns>
        protected override bool WantAccuSnap()
        {
            return base.WantAccuSnap();
        }
        /// <summary>
        /// Install后第一个进来的
        /// 1
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        protected override bool WantAdditionalLocate(DgnButtonEvent ev)
        {
            return base.WantAdditionalLocate(ev);
        }
        protected override bool WantAutoLocate()
        {
            return base.WantAutoLocate();
        }
        public override bool WantCheckGraphicGroupLock()
        {
            return base.WantCheckGraphicGroupLock();
        }
        protected override bool WantDynamics()
        {
            return base.WantDynamics();
        }
        public override bool WantMakeCopy()
        {
            return base.WantMakeCopy();
        }
    }
}
