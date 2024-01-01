import * as React from 'react';
import * as ReactDOM from 'react-dom';

import { HomeView } from "./read.component.tsx";
import UpdateView from "./update.component.tsx";
import CreateView from "./create.component.tsx";
import CatalogView from "./catalog.component.tsx";
import { LoginComponent } from "./login.component.tsx";

import {
    HashRouter,
    NavLink,
    Route
} from 'react-router-dom';

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
                        <Route path="/update" component={UpdateView}/>
                        <Route path="/create" component={CreateView}/>
                        <Route path="/catalog" component={CatalogView}/>
                    </div>
                </div>
            </HashRouter>);
    }
}

ReactDOM.render(
    <LoginComponent subscription={this} formId={null} jsonStorage={null} id={null}/>
    , document.querySelector("#renderLoginForm")
);

