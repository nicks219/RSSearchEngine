import {createPortal} from "react-dom";

export const Dialog = (props:{header:string, onAction:(action:string) => void, children:any}) => {
    const {onAction, children, header} = props;

    // рендерим портал только для более простого поиска при необходимости отладки
    return createPortal(
        <div className="dialog-overlay">
        <div className="dialog">
            <div>{header}!</div>
            <div>{children}</div>
            <div>
                <button onClick={()=>onAction('confirm')} className="btn btn-info">Да</button>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <button onClick={()=>onAction('dismiss')} className="btn btn-info">Нет</button>
            </div>
            <br/>
        </div></div>, document.body
    );
}
