import {createRoot} from "react-dom/client";
import {ReadView} from "./read.component.tsx";
import {UpdateView} from "./update.component.tsx";
import {CreateView} from "./create.component.tsx";
import {CatalogView} from "./catalog.component.tsx";
import {LoginComponent} from "./login.component.tsx";

import {
    HashRouter,
    NavLink,
    Routes,
    Route
} from 'react-router-dom';

export const MenuWithRouter = () => {
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
                            <Route path="/" element={<ReadView />}/>
                            <Route path="/read/:textId" element={<ReadView />}/>
                            <Route path="/update" element={<UpdateView />}/>
                            <Route path="/create" element={<CreateView />}/>
                            <Route path="/catalog" element={<CatalogView />}/>
                        </Routes>
                    </div>
                </div>
            </HashRouter>);
}

const renderLoginFormElement = document.getElementById("renderLoginForm") ?? document.createElement('renderLoginForm');
const renderRoot = createRoot(renderLoginFormElement);
renderRoot?.render(<LoginComponent />);

