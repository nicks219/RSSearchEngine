import * as React from "react";

export const LoginView = (props: {id:string, onClick:(e:React.SyntheticEvent)=>void}) => {
    return (
        <div id="login">
            <div id={props.id}>
                <input type="checkbox" id="loginButton" className="regular-checkbox" onClick={props.onClick}/>
                <label htmlFor="loginButton">Войти</label>
            </div>
            &nbsp;&nbsp;&nbsp;&nbsp;
            <span>
                <input type="text" id="email" name="email" autoComplete={"on"}/>
            </span>
            &nbsp;&nbsp;&nbsp;&nbsp;
            <span>
                <input type="text" id="password" name="password" autoComplete={"on"}/>
            </span>
        </div>
    );
}
