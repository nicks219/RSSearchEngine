import * as React from 'react';
import { createRoot } from "react-dom/client";

import { HomeView } from "../components/read.component.tsx";
import UpdateView from "../components/update.component.tsx";
import CreateView from "../components/create.component.tsx";
import CatalogView from "../components/catalog.component.tsx";
import { LoginComponent } from "../components/login.component.tsx";

import {
    HashRouter,
    NavLink,
    Routes,
    Route
} from 'react-router-dom';

declare global {
    interface Window { textId: number }
}

window.textId = 0;
window.React = React;

export const MenuRouter = () => {
        return (
            <HashRouter>
                <div>
                    <div id="header">
                        <ul>
                            <li><NavLink to="/">Посмотреть</NavLink></li>
                            <li><NavLink to="/update">Поменять</NavLink></li>
                            <li><NavLink to="/create">Создать</NavLink></li>
                            <li><NavLink to="/catalog">Каталог</NavLink></li>
                        </ul>
                    </div>

                    <div id="renderContainer1">
                        <Routes>
                            <Route path="/" element={<HomeView />}/>
                            <Route path="/read/:textId" element={<HomeView />}/>
                            <Route path="/update" element={<UpdateView formId={""} id={""} jsonStorage={""} subscription={""} />}/>
                            <Route path="/create" element={<CreateView formId={""} id={""} jsonStorage={""} subscription={""} />}/>
                            <Route path="/catalog" element={<CatalogView  subscription={""} />}/>
                        </Routes>
                    </div>
                </div>
            </HashRouter>);
}

const renderLoginFormElement = document.getElementById("renderLoginForm") ?? document.createElement('renderLoginForm');
createRoot(renderLoginFormElement).render(<LoginComponent subscription={this} formId={null} jsonStorage={null} id={null}/>);
