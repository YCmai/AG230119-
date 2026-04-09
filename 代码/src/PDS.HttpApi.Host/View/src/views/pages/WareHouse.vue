<template>
  <a-layout class="hb-layout">
    <a-layout-sider class="default-layout-sider">
      <menu-sider :name="$t('title.warehouse')" :menus="menus"></menu-sider>
    </a-layout-sider>
    <a-layout-content class="hb-layout-content">
      <router-view></router-view>
    </a-layout-content>
  </a-layout>
</template>
<script>
import "@/components/style/components.less";
import locale from "@/localize/warehouse/warehouse";
import menus from "@/menu/modules/ware-house";
export default {
  name: "WareHouse",
  i18n: {
    messages: locale,
  },
  data() {
    return {
      menus: menus,
      // menus: [
      //   {
      //     name: "list",
      //     displayName: this.$t("menus.list"),
      //     icon: "ordered-list",
      //     route: "/warehouse/list",
      //     isGranted: true,
      //     items: [],
      //   },
      // ],
    };
  },
};
</script>
