import * as React from 'react';
import {render} from "react-dom";
import * as ReactDOM from 'react-dom';

import { HomeView } from "./read";
import UpdateView from "./update";
import CreateView from "./create";
import CatalogView from "./catalog";
import { Login } from "./login";

import {
    HashRouter, 
    NavLink,
    Route
} from 'react-router-dom';
import {RouteComponentProps} from "react-router";

declare global {
    interface Window { textId: number }
}

window.textId = 0;
window.React = React;

export default class MenuRouter extends React.Component<any, any> {

    render() {
        return (
            <HashRouter>
                <div>
                    <div id="header">
                        <ul>
                            <li><NavLink exact to="/">Посмотреть</NavLink></li>
                            <li><NavLink to="/update">Поменять</NavLink></li>
                            <li><NavLink to="/create">Создать</NavLink></li>
                            <li><NavLink to="/catalog">Каталог</NavLink></li>
                        </ul>
                    </div>

                    
                    <div id="renderContainer1">
                        <Route exact path="/" component={HomeView}/>
                        <Route exact path="/read/:textId" component={HomeView}/>
                        
                        {/* для следующих пунктов меню требуется меньший сдвиг сверху: #renderContainer */}
                    
                        <Route path="/update" component={UpdateView}/>
                        <Route path="/create" component={CreateView}/>
                        <Route path="/catalog" component={CatalogView}/>
                    </div>
                </div>
            </HashRouter>);
    }
}

ReactDOM.render(
    <Login listener={this} formId={null} jsonStorage={null} id={null}/>
    , document.querySelector("#renderLoginForm")
);

