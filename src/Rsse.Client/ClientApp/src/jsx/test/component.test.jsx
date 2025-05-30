﻿import {describe, it, expect, vi, test, beforeEach, afterEach, beforeAll, afterAll} from 'vitest';
import {fireEvent, render as testingRender, screen} from '@testing-library/react';
import {App} from "../components/menu.component";
import {act} from "@testing-library/react";
import {createRoot} from "react-dom/client";
import {waitFor} from "@testing-library/dom";
import {Messages, SystemConstants} from "../dto/doms";

let container = null;
let root = null;

// https://vitest.dev/api/#beforeeach
beforeEach(() => {
    container = document.createElement("div");
    document.body.appendChild(container);
    root = createRoot(container);
})
beforeAll(() => {
    // https://vitest.dev/api/vi.html#vi-spyon
    // https://mswjs.io/docs/getting-started
    const fakeResponse = {response: "test"};
    // глобального fetch может не быть
    if (!global.fetch) {
        global.fetch = () => Promise.reject('fetch not available');
    }
    vi.spyOn(global, "fetch").mockImplementation(() =>
        Promise.resolve({
            json: () => Promise.resolve(fakeResponse)
        }))
})

// https://vitest.dev/api/#aftereach
afterEach(() => {
    root.unmount();
    container.remove();
    container = null;
})
afterAll(() => {
    // https://vitest.dev/api/vi.html#vi-restoreallmocks
    vi.restoreAllMocks()
})

describe('Menu Router component tests', () => {
    it.each([
        ["Посмотреть"],
        ["Поменять"],
        ["Создать"],
        ["Каталог"]
    ])
    ('menu element should render', (menuItemName) => {
        // arrange:
        // с рендером для screen добавленные данные "копятся" т.е. screen не очищается
        // testingRender(<App />);
        act (() => { root.render(<App />)});

        // act:
        let menuElement = screen.getByText(menuItemName);

        // assert:
        expect(menuElement).toBeInTheDocument();
    });
})

describe('Menu Navigate tests', () => {
    // https://vitest.dev/api/#test-each
    it.each([
        ["Каталог","#" + SystemConstants.catalogPath],
        ["Посмотреть","#" + SystemConstants.emptySegment],
        ["Поменять","#" + SystemConstants.updatePath],
        ["Создать","#" + SystemConstants.createPath],
    ])
    ('click on menu item should causes href changes', (menuItemName, uriExpected) => {
        // arrange: real rendering
        act (() => { root.render(<App />)});
        let collection = container.getElementsByTagName("a");
        for(let index in collection)
        {
            let element = collection[index];
            let inner = element.innerHTML;
            if (inner === menuItemName){
                // act:
                element.click();
                break;
            }
        }

        // assert:
        let href = document.location.href;
        expect(href).toContain(uriExpected);
    })
})

describe('Login Button tests', () => {
    // https://vitest.dev/api/#test-each
    it.each([
        [true, Messages.loginOk],
        [false, Messages.loginError]
    ])
    ('Login button click causes console message', async (response, expected) => {
        // arrange:
        const fakeResponse = {ok: response};
        vi.spyOn(global, "fetch").mockImplementation(() => Promise.resolve(fakeResponse))

        let message = null;
        act (() => { root.render(<App />)});
        console.log = function(msg){
            message = msg;
        };

        // act:
        let inputsElements = container.getElementsByTagName("input");
        for (let index in inputsElements)
        {
            let element = inputsElements[index];
            let attribute = element.attributes['type'];
            let value = attribute.value;
            if (value === 'checkbox')
            {
                await waitFor(() => element.click());
                break;
            }
        }

        // assert:
        expect(message).toBe(expected);
    })
})

// пиши тест на recovery либо на провайдеры контекста: "прерванная загрузка должна продолжаться после авторизации"
// то есть: login.container должен получать в useContext сохраненный в любом компоненте контекст и после loginOk использовать его в continueLoading
