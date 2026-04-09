<template>
  <a-layout class="hb-layout">
    <a-layout-sider class="default-layout-sider">
      <menu-sider :name="$t('title.goods')" :menus="menus"></menu-sider>
    </a-layout-sider>
    <a-layout-content class="hb-layout-content">
      <router-view></router-view>
    </a-layout-content>
  </a-layout>
</template>
<script>
import locale from "@/localize/locale";
export default {
  name: "Goods",
  i18n: {
    messages: locale,
  },
  data() {
    return {
      menus: [
        {
          name: "Materials",
          displayName: this.$t("goods.manage"),
          icon: "team",
          isGranted: true,
          items: [
            {
              name: "MaterialStore",
              displayName: this.$t("goods.store"),
              icon: "user",
              isGranted: true,
              route: "/goods/store",
            },
            {
              name: "Count",
              displayName: this.$t("goods.count"),
              icon: "idcard",
              isGranted: true,
              route: "/goods/count",
            },
            {
              name: "Maintain",
              displayName: this.$t("goods.maintain"),
              icon: "idcard",
              isGranted: true,
              route: "/goods/maintain",
            },
          ],
        },
        {
          name: "Transfer",
          displayName: this.$t("goods.transfer"),
          icon: "appstore",
          isGranted: true,
          items: [
            {
              name: "List",
              displayName: this.$t("goods.transferList"),
              icon: "api",
              isGranted: true,
              route: "/goods/transferList",
            },
          ],
        }
      ],
    };
  },
};
</script>
