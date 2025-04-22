import * as React from "react";
import {Doms, SystemConstants} from "../dto/doms.tsx";

export const LoginView = (props: {id:string, onClick:(e:React.SyntheticEvent)=>void}) => {
    return (
        <div id={Doms.loginName}>
            <div id={props.id}>
                <input type={Doms.checkbox} id={Doms.loginButton} className={Doms.regularCheckbox} onClick={props.onClick}/>
                <label htmlFor={Doms.loginButton}>Войти</label>
            </div>
            &nbsp;&nbsp;&nbsp;&nbsp;
            <span>
                <input type={Doms.text} id={Doms.email} name={Doms.email} autoComplete={SystemConstants.on}/>
            </span>
            &nbsp;&nbsp;&nbsp;&nbsp;
            <span>
                <input type={Doms.text} id={Doms.password} name={Doms.password} autoComplete={SystemConstants.on}/>
            </span>
        </div>
    );
}
