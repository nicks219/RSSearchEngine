import {createPortal} from "react-dom";
import {Doms, SystemConstants} from "../dto/doms.tsx";
import * as React from "react";

export const Dialog = (props:{header:string, onAction:(_action:string) => void, children:React.ReactNode}) => {
    const {onAction, children, header} = props;

    // рендерим портал только для более простого поиска при необходимости отладки
    return createPortal(
        <div className={Doms.dialogOverlay}>
        <div className={Doms.dialog}>
            <div>{header}!</div>
            <div>{children}</div>
            <div>
                <button onClick={()=>onAction(SystemConstants.confirm)} className={Doms.btnBtnInfo}>Да</button>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <button onClick={()=>onAction(SystemConstants.dismiss)} className={Doms.btnBtnInfo}>Нет</button>
            </div>
            <br/>
        </div></div>, document.body
    );
}
