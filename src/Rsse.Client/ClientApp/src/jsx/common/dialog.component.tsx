import {useEffect} from "react";

export const Dialog = (props:{header:string, onAction:(action:string) => void, children:any}) => {
    const {onAction, children, header} = props;

    const onMount = () => {
        document.body.classList.add('dialog-open')
    }
    const onUnmount = () => {
        document.body.classList.remove('dialog-open');
    }
    useEffect(() => {
        onMount();
        return onUnmount;
    }, [onAction])

    return (
        <div className="dialog">
            <div>{header}</div>
            <div>{children}</div>
            <div>
                <button onClick={()=>onAction('confirm')} className="btn btn-info">Да</button>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <button onClick={()=>onAction('dismiss')} className="btn btn-info">Нет</button>
            </div>
            <br/>
        </div>
    );
}
