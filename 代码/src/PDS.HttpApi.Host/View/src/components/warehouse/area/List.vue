<template>
  <div>
    <a-table
      :columns="columns"
      :data-source="data"
      bordered
      :rowKey="(row, index) => row.code"
      size="small"
      :pagination="false"
    >
      <template slot="operation" slot-scope="text, record">
        <a-space>
          <a href="javascript:void(0);" @click="show(record.code, ware)">
            <a-icon type="edit" />{{ $t("actions.edit") }}
          </a>
          <a href="javascript:void(0);" @click="deleteArea(record)">
            <a-icon type="edit" />{{ $t("actions.delete") }}
          </a>
        </a-space>
      </template>
    </a-table>
    <area-form-vue ref="areaForm" v-if="showAreaForm" @close="close" />
  </div>
</template>
<script>
import locale from "@/localize/warehouse/warehouse";
import AreaFormVue from "./Form.vue";
import { DeleteArea } from "@/api/inventory/area";
import component from "@/lib/base";
export default {
  name: "AreaList",
  i18n: {
    messages: locale,
  },
  mixins: [component],
  components: {
    AreaFormVue,
  },
  props: {
    wareHouse: {
      type: Object,
      default() {
        return {};
      },
    },
  },
  data() {
    return {
      columns: [
        {
          dataIndex: "code",
          title: "区域编码",
          width: 150,
        },
        {
          dataIndex: "name",
          title: "区域名称",
          width: 150,
        },
        {
          dataIndex: "category",
          title: "区域分类",
          width: 150,
        },
        {
          dataIndex: "description",
          title: "区域描述",
        },
        {
          dataIndex: "operations",
          title: this.$t("title.actions"),
          align: "center",
          scopedSlots: { customRender: "operation" },
          width: 200,
        },
      ],
      showAreaForm: false,
    };
  },
  computed: {
    data() {
      return this.wareHouse.areas;
    },
  },
  methods: {
    show: function (code) {
      this.showAreaForm = true;
      this.$nextTick(() => {
        this.$refs.areaForm.load(code, this.wareHouse);
      });
    },
    close: function () {
      this.showAreaForm = false;
      this.$emit("load");
    },
    deleteArea: async function (record) {
      if (!(await this.confirm("确定删除数据?"))) return;
      let spin = this.$spin({ text: "提交数据中...", target: ".content-layout-content" });
      try {
        await DeleteArea(this.wareHouse.id, record.code);
        this.$message.success("删除成功!");
        this.close();
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        spin.close();
      }
    },
  },
};
</script>
