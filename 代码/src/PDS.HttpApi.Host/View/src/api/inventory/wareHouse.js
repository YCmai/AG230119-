import axios from '@/lib/axios-extension';
import url from '@/lib/url-normalize';

export function CreateWareHouse(input) {
    return axios.post("/api/wms/warehouse/create", input)
}
export function UpdateWareHouse(id, input) {
    return axios.put("/api/wms/warehouse/" + id, input);
}
export function DeleteWareHouse(id) {
    return axios.delete(`/api/wms/warehouse/${id}`);
}
export function GetAllWareHouses() {
    return axios.get("/api/wms/warehouse/all");
}
export function GetWareHouses(input) {
    return axios.get("/api/wms/warehouse/page?" + url.normalize(input))
}
export function GetWareHouse(id) {
    return axios.get(`/api/wms/warehouse/${id}`);
}